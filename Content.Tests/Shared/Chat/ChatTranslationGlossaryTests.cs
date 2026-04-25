// CM14 rework: non-RMC edit marker.
using Content.Shared.Chat;
using NUnit.Framework;

namespace Content.Tests.Shared.Chat;

[TestFixture]
public sealed class ChatTranslationGlossaryTests
{
    [Test]
    public void TryTranslateDirect_TranslatesCommonChatPhrase_WithPunctuation()
    {
        var translated = ChatTranslationGlossary.TryTranslateDirect("go away!", "ru", out var result);

        Assert.Multiple(() =>
        {
            Assert.That(translated, Is.True);
            Assert.That(result, Is.EqualTo("отойди!"));
        });
    }

    [Test]
    public void PrepareForTranslation_ProtectsRmcTerms()
    {
        var prepared = ChatTranslationGlossary.PrepareForTranslation("Go to medbay and call CAS.", "ru");

        Assert.Multiple(() =>
        {
            Assert.That(prepared.Text, Does.Not.Contain("medbay"));
            Assert.That(prepared.Text, Does.Not.Contain("CAS"));
            Assert.That(prepared.ProtectedTerms, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void ApplyPostProcessing_RestoresProtectedRmcTerms()
    {
        var prepared = ChatTranslationGlossary.PrepareForTranslation("Go to medbay and call CAS.", "ru");
        var translated = ChatTranslationGlossary.ApplyPostProcessing(
            "Go to medbay and call CAS.",
            $"Иди в {prepared.ProtectedTerms[0].Token} и вызови {prepared.ProtectedTerms[1].Token}.",
            "ru",
            prepared);

        Assert.That(translated, Is.EqualTo("Иди в медбей и вызови CAS."));
    }
}
