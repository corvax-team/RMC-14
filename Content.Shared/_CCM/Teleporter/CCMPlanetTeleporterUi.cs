using Robust.Shared.Serialization;
using Robust.Shared.Maths;
using Content.Shared.UserInterface;

namespace Content.Shared._CCM.Teleporter;

[Serializable, NetSerializable]
public enum CCMPlanetTeleporterUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CCMPlanetTeleporterState(NetEntity mapEntity, string? mapName, bool teleported, TimeSpan cooldownRemaining)
    : BoundUserInterfaceState
{
    public readonly NetEntity MapEntity = mapEntity;
    public readonly string? MapName = mapName;
    public readonly bool Teleported = teleported;
    public readonly TimeSpan CooldownRemaining = cooldownRemaining;
}

[Serializable, NetSerializable]
public sealed class CCMPlanetTeleporterSelectMsg(Vector2i position) : BoundUserInterfaceMessage
{
    public readonly Vector2i Position = position;
}
