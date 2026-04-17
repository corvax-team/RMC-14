using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

[Serializable, NetSerializable]
public sealed class CCMGhostTakeParasiteExamineState : BoundUserInterfaceState
{
    public NetEntity Actor { get; }
    public bool IsRoyal { get; }

    public CCMGhostTakeParasiteExamineState(NetEntity actor, bool isRoyal)
    {
        Actor = actor;
        IsRoyal = isRoyal;
    }
}
