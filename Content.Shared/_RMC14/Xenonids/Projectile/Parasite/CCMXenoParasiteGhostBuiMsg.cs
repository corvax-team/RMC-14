using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

[Serializable, NetSerializable]
public sealed class CCMXenoParasiteGhostBuiMsg : BoundUserInterfaceMessage
{
    public NetEntity Actor { get; }

    public CCMXenoParasiteGhostBuiMsg(EntityUid actor, IEntityManager? entityManager = null)
    {
        if (entityManager != null)
            Actor = entityManager.GetNetEntity(actor);
        else
            Actor = new NetEntity(actor.Id);
    }
}
