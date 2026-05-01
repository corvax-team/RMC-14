// CM14 rework: non-RMC edit marker.
using System;
using System.Text;
using System.Text.RegularExpressions;
using Robust.Shared.Utility;

namespace Content.Shared.Chat;

public static class ChatTranslationMarkup
{
    private static readonly Regex RichMarkupTagRegex = new(
        @"\[(\/)?[a-z]+(?:[ =][^\]]+)?\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string BuildTranslatedMarkup(string translatedMessage, bool italicize = true)
    {
        var escapedTranslated = EscapeTextPreservingMarkup(translatedMessage);
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

    private static string EscapeTextPreservingMarkup(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var builder = new StringBuilder(text.Length);
        var lastIndex = 0;

        foreach (Match match in RichMarkupTagRegex.Matches(text))
        {
            if (match.Index > lastIndex)
                builder.Append(FormattedMessage.EscapeText(text[lastIndex..match.Index]));

            builder.Append(match.Value);
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
            builder.Append(FormattedMessage.EscapeText(text[lastIndex..]));

        return builder.ToString();
    }
}
