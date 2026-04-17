using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

[Serializable, NetSerializable]
public sealed class CCMXenoParasiteGhostBuiMsg : BoundUserInterfaceMessage
{
    public NetEntity Actor { get; }

    public CCMXenoParasiteGhostBuiMsg(NetEntity actor)
    {
        Actor = actor;
    }
}
