// CM14 rework: non-RMC edit marker.
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._MC;
using Content.Shared._RMC14.CCVar;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Managers;

internal sealed partial class ChatManager
{
    [GeneratedRegex(@"\[(\/)?[a-z]+(?:[ =][^\]]+)?\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RichTextTagRegex();

    [GeneratedRegex(
        @"^(?:Admin (?:login|logout):|Админ (?:заш[её]л|вышел):|SERVER:\s+.+\s+has\s+(?:disconnected|reconnected(?:\s+to\s+the\s+server)?|connected)\.?)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NonTranslatableSystemStatusRegex();

    [Dependency] private readonly IHttpClientHolder _http = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;

    private const ChatChannel TranslatableChannels =
        ChatChannel.Local |
        ChatChannel.Whisper |
        ChatChannel.Server |
        ChatChannel.Radio |
        ChatChannel.LOOC |
        ChatChannel.OOC |
        ChatChannel.Notifications |
        ChatChannel.Dead |
        ChatChannel.Admin |
        ChatChannel.AdminAlert |
        ChatChannel.AdminChat |
        ChatChannel.MentorChat;

    private readonly ConcurrentDictionary<(string Api, string Source, string Target, string Text), Task<string?>> _translationCache = new();
#if DEBUG
    private readonly ConcurrentDictionary<string, bool> _debugUnavailableTranslateApis = new();
#endif

    private void DispatchChatMessageToClient(INetChannel client, ChatMessage message)
    {
        var outgoing = CloneChatMessage(message);
        if (!TryBuildTranslationRequest(client, outgoing, out var request))
        {
            _netManager.ServerSendMessage(new MsgChatMessage { Message = outgoing }, client);
            return;
        }

        SendTranslatedChatMessage(client, outgoing, request);
    }

    private async void SendTranslatedChatMessage(INetChannel client, ChatMessage message, TranslationRequest request)
    {
        try
        {
            var translated = await GetTranslatedMessageAsync(request);
            if (!string.IsNullOrWhiteSpace(translated))
                message = ApplyTranslatedMessage(message, translated);
        }
        catch (Exception e)
        {
            Logger.WarningS("chat.translate", $"Failed to translate chat message for {client}: {e}");
        }
        finally
        {
            _taskManager.RunOnMainThread(() =>
            {
                _netManager.ServerSendMessage(new MsgChatMessage { Message = message }, client);
            });
        }
    }

    private void RecordReplayIfNeeded(ChatMessage message, bool recordReplay)
    {
        if (!recordReplay)
            return;

        if ((message.Channel & ChatChannel.AdminRelated) != 0 &&
            !_configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
        {
            return;
        }

        _replay.RecordServerMessage(message);
    }

    private bool TryBuildTranslationRequest(INetChannel client, ChatMessage message, out TranslationRequest request)
    {
        request = default;

        if ((message.Channel & TranslatableChannels) == 0)
        {
            Logger.InfoS("chat.translate", $"Skip chat translation for {client}: channel {message.Channel} is not translatable.");
            return false;
        }

        if (!_netConfigManager.GetClientCVar(client, RMCCVars.RMCChatTranslateEnabled))
        {
            Logger.InfoS("chat.translate", $"Skip chat translation for {client}: rmc.chat_translate_enabled = false.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(message.TranslatedMessage))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: message is already localized/translated. Message='{message.Message}'");
            return false;
        }

        if (ShouldSkipAdministrativeSystemTranslation(message))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: administrative system message should not be translated. Channel={message.Channel}, Message='{message.Message}'");
            return false;
        }

        if (ShouldSkipSystemStatusTranslation(message))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: connection/admin status system message should not be translated. Channel={message.Channel}, Message='{message.Message}'");
            return false;
        }

        if (ShouldSkipAresTranslation(message))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: ARES announcement should not be translated. Channel={message.Channel}, Message='{message.Message}'");
            return false;
        }

        if (string.IsNullOrWhiteSpace(message.Message) || !message.Message.Any(char.IsLetter))
        {
            Logger.InfoS("chat.translate", $"Skip chat translation for {client}: message has no translatable text. Message='{message.Message}'");
            return false;
        }

        var api = NormalizeTranslateApiEndpoint(_configurationManager.GetCVar(RMCCVars.RMCChatTranslateApi));
        var source = NormalizeLanguageCode(_configurationManager.GetCVar(RMCCVars.RMCChatTranslateSource), allowAuto: true);
        var target = NormalizeLanguageCode(_netConfigManager.GetClientCVar(client, RMCCVars.RMCChatTranslateTarget), allowAuto: false);

        if (api == null || source == null || target == null)
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: invalid translation config. api='{api}', source='{source}', target='{target}'.");
            return false;
        }

#if DEBUG
        // CCM translation debug guard: don't keep trying a dead local LibreTranslate on debug-local servers.
        if (_debugUnavailableTranslateApis.ContainsKey(api) && IsLoopbackTranslateApi(api))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: local debug LibreTranslate '{api}' is marked unavailable.");
            return false;
        }
#endif

        if (LooksLikeTargetLanguage(message.Message, target))
        {
            Logger.InfoS("chat.translate",
                $"Skip chat translation for {client}: message already looks like target language '{target}'. Message='{message.Message}'");
            return false;
        }

        request = new TranslationRequest(api, source, target, message.Message);
        Logger.InfoS("chat.translate",
            $"Queue chat translation for {client}: source='{source}', target='{target}', api='{api}', message='{message.Message}'");
        return true;
    }

    private async Task<string?> GetTranslatedMessageAsync(TranslationRequest request)
    {
        if (ChatTranslationGlossary.TryTranslateDirect(request.Text, request.Target, out var directTranslation))
            return directTranslation;

        var prepared = ChatTranslationGlossary.PrepareForTranslation(request.Text, request.Target);
        var task = _translationCache.GetOrAdd((request.Api, request.Source, request.Target, prepared.Text), key =>
            TranslateChatMessageAsync(key.Api, key.Source, key.Target, key.Text));

        var translated = await task;
        return translated == null
            ? null
            : ChatTranslationGlossary.ApplyPostProcessing(request.Text, translated, request.Target, prepared);
    }

    private async Task<string?> TranslateChatMessageAsync(string api, string source, string target, string text)
    {
        using var cts = new CancellationTokenSource(GetTranslateTimeout(api));
        using var request = new HttpRequestMessage(HttpMethod.Post, api)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new LibreTranslateRequest(text, source, target)),
                Encoding.UTF8,
                "application/json")
        };

        Logger.InfoS("chat.translate", $"Sending LibreTranslate request to '{api}' with source='{source}', target='{target}', text='{text}'");
        HttpResponseMessage response;
        try
        {
            response = await _http.Client.SendAsync(request, cts.Token);
        }
        catch (Exception e) when (IsDebugLocalTranslateFailure(api, e))
        {
#if DEBUG
            if (_debugUnavailableTranslateApis.TryAdd(api, true))
            {
                Logger.InfoS("chat.translate",
                    $"Marked local debug LibreTranslate '{api}' as unavailable after connection failure: {e.Message}");
            }
#endif
            return null;
        }

        using (response)
        {
        if (!response.IsSuccessStatusCode)
        {
            Logger.WarningS("chat.translate",
                $"LibreTranslate returned non-success status {(int) response.StatusCode} for api='{api}', target='{target}'");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        var payload = await JsonSerializer.DeserializeAsync<LibreTranslateResponse>(stream, cancellationToken: cts.Token);
        var translated = payload?.TranslatedText?.Trim();

        if (string.IsNullOrWhiteSpace(translated))
        {
            Logger.InfoS("chat.translate", $"LibreTranslate returned empty translation for text='{text}'");
            return null;
        }

        Logger.InfoS("chat.translate", $"LibreTranslate response for text='{text}': '{translated}'");
        return string.Equals(translated, text.Trim(), StringComparison.Ordinal)
            ? null
            : translated;
        }
    }

    private static ChatMessage CloneChatMessage(ChatMessage message)
    {
        return new ChatMessage(
            message.Channel,
            message.Message,
            message.WrappedMessage,
            message.SenderEntity,
            message.SenderKey,
            message.HideChat,
            message.MessageColorOverride,
            message.AudioPath,
            message.AudioVolume,
            message.HidePopup,
            message.SpeechStyleClass,
            message.RepeatCheckSender,
            message.TranslatedMessage);
    }

    private static ChatMessage ApplyTranslatedMessage(ChatMessage message, string translated)
    {
        return new ChatMessage(
            message.Channel,
            translated,
            ChatTranslationMarkup.ApplyTranslatedWrappedMessage(message.WrappedMessage, message.Message, translated),
            message.SenderEntity,
            message.SenderKey,
            message.HideChat,
            message.MessageColorOverride,
            message.AudioPath,
            message.AudioVolume,
            message.HidePopup,
            message.SpeechStyleClass,
            message.RepeatCheckSender,
            translatedMessage: translated);
    }

    private static string? NormalizeTranslateApiEndpoint(string raw)
    {
        raw = raw.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (!raw.Contains("://", StringComparison.Ordinal))
            raw = $"http://{raw}";

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            return null;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var builder = new UriBuilder(uri);
        var path = builder.Path.TrimEnd('/');
        builder.Path = path.EndsWith("/translate", StringComparison.OrdinalIgnoreCase)
            ? path
            : string.IsNullOrWhiteSpace(path)
                ? "/translate"
                : $"{path}/translate";

        return builder.Uri.ToString();
    }

    private static bool ContainsRichMarkup(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && RichTextTagRegex().IsMatch(text);
    }

    private static bool ContainsUnsupportedRichMarkup(string text)
    {
        if (!ContainsRichMarkup(text))
            return false;

        var matches = RichTextTagRegex().Matches(text);
        foreach (Match match in matches)
        {
            if (!MCFormatMessage.IsKnownBracketEmojiTag(match.Value))
                return true;
        }

        return false;
    }

    private static bool ShouldSkipAdministrativeSystemTranslation(ChatMessage message)
    {
        return (message.Channel & ChatChannel.AdminRelated) != 0 &&
               message.SenderEntity == NetEntity.Invalid &&
               message.SenderKey == null;
    }

    private static bool ShouldSkipSystemStatusTranslation(ChatMessage message)
    {
        if (message.SenderEntity != NetEntity.Invalid || message.SenderKey != null)
            return false;

        var isSystemChannel = (message.Channel & (ChatChannel.Server | ChatChannel.Admin | ChatChannel.AdminAlert | ChatChannel.AdminChat | ChatChannel.AdminRelated)) != 0;
        if (!isSystemChannel)
            return false;

        return !string.IsNullOrWhiteSpace(message.Message) &&
               NonTranslatableSystemStatusRegex().IsMatch(message.Message.Trim());
    }

    private static bool ShouldSkipAresTranslation(ChatMessage message)
    {
        if (message.SenderEntity != NetEntity.Invalid || message.SenderKey != null)
            return false;

        if ((message.Channel & ChatChannel.Radio) == 0)
            return false;

        if (!ContainsUnsupportedRichMarkup(message.Message))
            return false;

        var plain = RichTextTagRegex().Replace(message.Message, string.Empty);
        return plain.Contains("ARES", StringComparison.OrdinalIgnoreCase) ||
               plain.Contains("АРЕС", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeLanguageCode(string raw, bool allowAuto)
    {
        raw = raw.Trim().ToLowerInvariant();

        if (allowAuto && (string.IsNullOrWhiteSpace(raw) || raw == "auto"))
            return "auto";

        return raw switch
        {
            "ru" or "ru-ru" => "ru",
            "en" or "en-us" or "en-gb" => "en",
            _ => null,
        };
    }

    private static bool LooksLikeTargetLanguage(string text, string target)
    {
        var hasCyrillic = false;
        var hasLatin = false;

        foreach (var rune in text.EnumerateRunes())
        {
            if (!Rune.IsLetter(rune))
                continue;

            var value = rune.Value;
            if (value is >= 0x0400 and <= 0x04FF)
                hasCyrillic = true;
            else if ((value is >= 'A' and <= 'Z') || (value is >= 'a' and <= 'z'))
                hasLatin = true;
        }

        return target switch
        {
            "ru" => hasCyrillic && !hasLatin,
            "en" => hasLatin && !hasCyrillic,
            _ => false,
        };
    }

    private static TimeSpan GetTranslateTimeout(string api)
    {
#if DEBUG
        if (IsLoopbackTranslateApi(api))
            return TimeSpan.FromMilliseconds(350);
#endif
        return TimeSpan.FromSeconds(3);
    }

    private static bool IsDebugLocalTranslateFailure(string api, Exception exception)
    {
#if DEBUG
        return IsLoopbackTranslateApi(api) &&
               (exception is HttpRequestException or TaskCanceledException or OperationCanceledException);
#else
        return false;
#endif
    }

    private static bool IsLoopbackTranslateApi(string api)
    {
        if (!Uri.TryCreate(api, UriKind.Absolute, out var uri))
            return false;

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return IPAddress.TryParse(uri.Host, out var address) && IPAddress.IsLoopback(address);
    }

    private readonly record struct TranslationRequest(string Api, string Source, string Target, string Text);

    private sealed record LibreTranslateRequest(
        [property: JsonPropertyName("q")] string Text,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("target")] string Target,
        [property: JsonPropertyName("format")] string Format = "text");

    private sealed record LibreTranslateResponse(
        [property: JsonPropertyName("translatedText")] string? TranslatedText);
}
