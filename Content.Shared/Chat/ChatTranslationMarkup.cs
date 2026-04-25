// CM14 rework: non-RMC edit marker.
using System;
using Robust.Shared.Utility;

namespace Content.Shared.Chat;

public static class ChatTranslationMarkup
{
    public static string BuildTranslatedMarkup(string translatedMessage, bool italicize = true)
    {
        var escapedTranslated = FormattedMessage.EscapeText(translatedMessage);
        return italicize
            ? $"[italic]{escapedTranslated}[/italic]"
            : escapedTranslated;
    }

    public static string ReplaceWrappedMessageText(string wrappedMessage, string originalMessage, string replacementMarkup)
    {
        var escapedOriginal = FormattedMessage.EscapeText(originalMessage);
        var index = wrappedMessage.LastIndexOf(escapedOriginal, StringComparison.Ordinal);
        if (index == -1)
            return replacementMarkup;

        return string.Concat(
            wrappedMessage.AsSpan(0, index),
            replacementMarkup,
            wrappedMessage.AsSpan(index + escapedOriginal.Length));
    }

    public static string ApplyTranslatedWrappedMessage(string wrappedMessage, string originalMessage, string translatedMessage, bool italicize = true)
    {
        return ReplaceWrappedMessageText(
            wrappedMessage,
            originalMessage,
            BuildTranslatedMarkup(translatedMessage, italicize));
    }
}
