using System.IO;
using System.Text;
using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;

namespace Content.IntegrationTests.Tests.Lobby;

[TestFixture]
[TestOf(typeof(HumanoidAppearanceSystem))]
public sealed class CharacterProfileImportTest
{
    [Test]
    public async Task ImportLegacyRawProfileTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { InLobby = true });
        await pair.RunTicksSync(1);

        Assert.That(pair.Player, Is.Not.Null);

        await pair.Server.WaitAssertion(() =>
        {
            var serManager = pair.Server.ResolveDependency<ISerializationManager>();
            var humanoid = pair.Server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<HumanoidAppearanceSystem>();

            var original = new HumanoidCharacterProfile
            {
                Name = "Legacy Tester",
                Age = 37,
                Species = SharedHumanoidAppearanceSystem.DefaultSpecies,
            };

            var rawProfile = serManager.WriteValue(original, alwaysWrite: true, notNullableOverride: true);
            using var stream = ToStream(rawProfile);
            var imported = humanoid.FromStream(stream, pair.Player!);

            Assert.Multiple(() =>
            {
                Assert.That(imported.Name, Is.EqualTo(original.Name));
                Assert.That(imported.Age, Is.EqualTo(original.Age));
                Assert.That(imported.Species, Is.EqualTo(original.Species));
            });
        });
    }

    [Test]
    public async Task ImportWrappedProfileWithUnsupportedDataTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { InLobby = true });
        await pair.RunTicksSync(1);

        Assert.That(pair.Player, Is.Not.Null);

        await pair.Server.WaitAssertion(() =>
        {
            var humanoid = pair.Server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<HumanoidAppearanceSystem>();

            var original = new HumanoidCharacterProfile
            {
                Name = "Future Tester",
                Age = 29,
                Species = SharedHumanoidAppearanceSystem.DefaultSpecies,
            };

            var exported = humanoid.ToDataNode(original);
            Assert.That(exported, Is.InstanceOf<MappingDataNode>());

            var exportMapping = (MappingDataNode) exported;
            exportMapping.Add("futureRootField", new ValueDataNode("ignored"));

            var profileMapping = exportMapping.Get<MappingDataNode>("profile");
            profileMapping["gender"] = new ValueDataNode("FutureGender");
            profileMapping["sex"] = new ValueDataNode("FutureSex");
            profileMapping.Add("futureProfileField", new ValueDataNode("ignored"));

            using var stream = ToStream(exportMapping);
            var imported = humanoid.FromStream(stream, pair.Player!);

            Assert.Multiple(() =>
            {
                Assert.That(imported.Name, Is.EqualTo(original.Name));
                Assert.That(imported.Age, Is.EqualTo(original.Age));
                Assert.That(imported.Species, Is.EqualTo(original.Species));
                Assert.That(imported.Gender, Is.EqualTo(Gender.Male));
                Assert.That(imported.Sex, Is.EqualTo(Sex.Male));
            });
        });
    }

    private static MemoryStream ToStream(DataNode node)
    {
        using var writer = new StringWriter();
        node.Write(writer);

        return new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString()));
    }
}
