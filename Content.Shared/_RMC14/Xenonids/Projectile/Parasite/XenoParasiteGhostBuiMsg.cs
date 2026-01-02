using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

[Serializable, NetSerializable]
public sealed class XenoParasiteGhostBuiMsg : BoundUserInterfaceMessage
{
    public uint ActorId { get; }

    public XenoParasiteGhostBuiMsg(uint actorId)
    {
        ActorId = actorId;
    }
}
