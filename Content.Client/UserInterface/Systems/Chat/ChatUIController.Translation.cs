using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._RMC14.CCVar;
using Content.Shared.Chat;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;

namespace Content.Client.UserInterface.Systems.Chat;

public sealed partial class ChatUIController
{
    [Dependency] private readonly IHttpClientHolder _http = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;

    private const ChatChannel TranslatableChannels =
        ChatChannel.Local |
        ChatChannel.Whisper |
        ChatChannel.Radio |
        ChatChannel.LOOC |
        ChatChannel.OOC |
        ChatChannel.Dead;

    private readonly Dictionary<ChatMessage, string> _translatedMessages = new();
    private readonly HashSet<ChatMessage> _completedTranslations = new();
    private readonly Dictionary<(string Api, string Source, string Target, string Text), Task<string?>> _inflightTranslations = new();

    public bool TryGetTranslatedMessage(ChatMessage msg, out string translated)
    {
        return _translatedMessages.TryGetValue(msg, out translated!);
    }

    public void QueueChatTranslation(ChatMessage msg)
    {
        if (!ShouldTranslateMessage(msg) || _completedTranslations.Contains(msg))
            return;

        var api = _config.GetCVar(RMCCVars.RMCChatTranslateApi).Trim();
        var source = _config.GetCVar(RMCCVars.RMCChatTranslateSource).Trim();
        var target = _config.GetCVar(RMCCVars.RMCChatTranslateTarget).Trim();

        if (string.IsNullOrWhiteSpace(api) ||
            string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(target))
        {
            _completedTranslations.Add(msg);
            return;
        }

        var requestKey = (api, source, target, msg.Message);
        if (!_inflightTranslations.TryGetValue(requestKey, out var task))
        {
            task = TranslateChatMessageAsync(api, source, target, msg.Message);
            _inflightTranslations[requestKey] = task;
        }

        _ = CompleteChatTranslationAsync(msg, requestKey, task);
    }

    private bool ShouldTranslateMessage(ChatMessage msg)
    {
        if (!_config.GetCVar(RMCCVars.RMCChatTranslateEnabled))
            return false;

        if ((msg.Channel & TranslatableChannels) == 0)
            return false;

        if (string.IsNullOrWhiteSpace(msg.Message))
            return false;

        return msg.Message.Any(char.IsLetter);
    }

    private async Task CompleteChatTranslationAsync(
        ChatMessage msg,
        (string Api, string Source, string Target, string Text) requestKey,
        Task<string?> task)
    {
        try
        {
            var translated = await task;

            _taskManager.RunOnMainThread(() =>
            {
                _completedTranslations.Add(msg);

                if (string.IsNullOrWhiteSpace(translated))
                    return;

                _translatedMessages[msg] = translated;
                Repopulate();
            });
        }
        catch
        {
            _taskManager.RunOnMainThread(() => _completedTranslations.Add(msg));
        }
        finally
        {
            _taskManager.RunOnMainThread(() =>
            {
                if (_inflightTranslations.TryGetValue(requestKey, out var current) && current == task)
                    _inflightTranslations.Remove(requestKey);
            });
        }
    }

    private async Task<string?> TranslateChatMessageAsync(string api, string source, string target, string text)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var request = new HttpRequestMessage(HttpMethod.Post, api)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new LibreTranslateRequest(text, source, target)),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await _http.Client.SendAsync(request, cts.Token);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        var payload = await JsonSerializer.DeserializeAsync<LibreTranslateResponse>(stream, cancellationToken: cts.Token);
        var translated = payload?.TranslatedText?.Trim();

        if (string.IsNullOrWhiteSpace(translated))
            return null;

        return string.Equals(translated, text.Trim(), StringComparison.Ordinal)
            ? null
            : translated;
    }

    private sealed record LibreTranslateRequest(
        [property: JsonPropertyName("q")] string Text,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("target")] string Target,
        [property: JsonPropertyName("format")] string Format = "text");

    private sealed record LibreTranslateResponse(
        [property: JsonPropertyName("translatedText")] string? TranslatedText);
}
