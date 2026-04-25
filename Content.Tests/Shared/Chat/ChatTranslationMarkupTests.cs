// CM14 rework: non-RMC edit marker.
using Content.Shared.Chat;
using NUnit.Framework;

namespace Content.Tests.Shared.Chat;

[TestFixture]
public sealed class ChatTranslationMarkupTests
{
    [Test]
    public void ApplyTranslatedWrappedMessage_ReplacesOnlyMessageText_WithItalicMarkup()
    {
        var wrapped = "[bold]John[/bold] says, \"Hello there\"";
        var result = ChatTranslationMarkup.ApplyTranslatedWrappedMessage(wrapped, "Hello there", "Привет");

        Assert.That(result, Is.EqualTo("[bold]John[/bold] says, \"[italic]Привет[/italic]\""));
    }

    [Test]
    public void ApplyTranslatedWrappedMessage_PreservesExistingOuterTags()
    {
        var wrapped = "[BubbleHeader]John[/BubbleHeader][BubbleContent]Hello there[/BubbleContent]";
        var result = ChatTranslationMarkup.ApplyTranslatedWrappedMessage(wrapped, "Hello there", "Привет");

        Assert.That(result, Is.EqualTo("[BubbleHeader]John[/BubbleHeader][BubbleContent][italic]Привет[/italic][/BubbleContent]"));
    }

    [Test]
    public void ApplyTranslatedWrappedMessage_FallsBackToTranslatedMarkup_WhenOriginalNotFound()
    {
        var result = ChatTranslationMarkup.ApplyTranslatedWrappedMessage("[color=red]ignored[/color]", "Hello there", "Привет");

        Assert.That(result, Is.EqualTo("[italic]Привет[/italic]"));
    }
}
