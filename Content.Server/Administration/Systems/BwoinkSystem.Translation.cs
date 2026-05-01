// CM14 rework: non-RMC edit marker.
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._RMC14.CCVar;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server.Administration.Systems;

public sealed partial class BwoinkSystem
{
    [Dependency] private readonly IHttpClientHolder _translationHttp = default!;
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;
    [Dependency] private readonly ITaskManager _task = default!;

    private readonly ConcurrentDictionary<(string Api, string Source, string Target, string Text), Task<string?>> _translationCache = new();
#if DEBUG
    private readonly ConcurrentDictionary<string, bool> _debugUnavailableTranslateApis = new();
#endif

    private void SendBwoinkMessage(INetChannel channel, BwoinkTextMessage message)
    {
        RaiseNetworkEvent(CloneBwoinkMessage(message), channel);
    }

    private async void SendTranslatedBwoinkMessage(INetChannel channel, BwoinkTextMessage message, TranslationRequest request)
    {
        try
        {
            var translated = await GetTranslatedMessageAsync(request);
            if (!string.IsNullOrWhiteSpace(translated))
                message = CloneBwoinkMessage(message, translated);
        }
        catch (Exception e)
        {
            _sawmill.Warning($"Failed to translate bwoink for {channel}: {e}");
        }
        finally
        {
            _task.RunOnMainThread(() => RaiseNetworkEvent(message, channel));
        }
    }

    private bool TryBuildTranslationRequest(INetChannel channel, string text, out TranslationRequest request)
    {
        request = default;

        if (!_config.GetCVar(RMCCVars.RMCChatTranslateApi).Any())
        {
            _sawmill.Info("Skip bwoink translation: rmc.chat_translate_api is empty.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(text) || !text.Any(char.IsLetter))
        {
            _sawmill.Info($"Skip bwoink translation for {channel}: text has no translatable content. Text='{text}'");
            return false;
        }

        var api = NormalizeTranslateApiEndpoint(_config.GetCVar(RMCCVars.RMCChatTranslateApi));
        var source = NormalizeLanguageCode(_config.GetCVar(RMCCVars.RMCChatTranslateSource), allowAuto: true);
        var target = NormalizeLanguageCode(_netConfig.GetClientCVar(channel, RMCCVars.RMCChatTranslateTarget), allowAuto: false);

        if (api == null || source == null || target == null)
        {
            _sawmill.Info($"Skip bwoink translation for {channel}: invalid translation config. api='{api}', source='{source}', target='{target}'.");
            return false;
        }

#if DEBUG
        // CCM translation debug guard: don't keep trying a dead local LibreTranslate on debug-local servers.
        if (_debugUnavailableTranslateApis.ContainsKey(api) && IsLoopbackTranslateApi(api))
        {
            _sawmill.Info($"Skip bwoink translation for {channel}: local debug LibreTranslate '{api}' is marked unavailable.");
            return false;
        }
#endif

        if (LooksLikeTargetLanguage(text, target))
        {
            _sawmill.Info($"Skip bwoink translation for {channel}: text already looks like target language '{target}'. Text='{text}'");
            return false;
        }

        request = new TranslationRequest(api, source, target, text);
        _sawmill.Info($"Queue bwoink translation for {channel}: source='{source}', target='{target}', api='{api}', text='{text}'");
        return true;
    }

    private async Task<string?> GetTranslatedMessageAsync(TranslationRequest request)
    {
        if (ChatTranslationGlossary.TryTranslateDirect(request.Text, request.Target, out var directTranslation))
            return directTranslation;

        var prepared = ChatTranslationGlossary.PrepareForTranslation(request.Text, request.Target);
        var task = _translationCache.GetOrAdd((request.Api, request.Source, request.Target, prepared.Text), key =>
            TranslateAsync(key.Api, key.Source, key.Target, key.Text));

        var translated = await task;
        return translated == null
            ? null
            : ChatTranslationGlossary.ApplyPostProcessing(request.Text, translated, request.Target, prepared);
    }

    private async Task<string?> TranslateAsync(string api, string source, string target, string text)
    {
        using var cts = new CancellationTokenSource(GetTranslateTimeout(api));
        using var request = new HttpRequestMessage(HttpMethod.Post, api)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new LibreTranslateRequest(text, source, target)),
                Encoding.UTF8,
                "application/json")
        };

        _sawmill.Info($"Sending LibreTranslate bwoink request to '{api}' with source='{source}', target='{target}', text='{text}'");
        HttpResponseMessage response;
        try
        {
            response = await _translationHttp.Client.SendAsync(request, cts.Token);
        }
        catch (Exception e) when (IsDebugLocalTranslateFailure(api, e))
        {
#if DEBUG
            if (_debugUnavailableTranslateApis.TryAdd(api, true))
            {
                _sawmill.Info($"Marked local debug LibreTranslate '{api}' as unavailable after connection failure: {e.Message}");
            }
#endif
            return null;
        }

        using (response)
        {
        if (!response.IsSuccessStatusCode)
        {
            _sawmill.Warning($"LibreTranslate bwoink request returned status {(int) response.StatusCode} for api='{api}', target='{target}'");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        var payload = await JsonSerializer.DeserializeAsync<LibreTranslateResponse>(stream, cancellationToken: cts.Token);
        var translated = payload?.TranslatedText?.Trim();

        if (string.IsNullOrWhiteSpace(translated))
        {
            _sawmill.Info($"LibreTranslate returned empty bwoink translation for text='{text}'");
            return null;
        }

        _sawmill.Info($"LibreTranslate bwoink response for text='{text}': '{translated}'");
        return string.Equals(translated, text.Trim(), StringComparison.Ordinal)
            ? null
            : translated;
        }
    }

    private static BwoinkTextMessage CloneBwoinkMessage(BwoinkTextMessage message, string? translatedText = null)
    {
        return new BwoinkTextMessage(
            message.UserId,
            message.TrueSender,
            translatedText ?? message.Text,
            message.SentAt,
            message.PlaySound,
            message.AdminOnly,
            translatedText: translatedText);
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
