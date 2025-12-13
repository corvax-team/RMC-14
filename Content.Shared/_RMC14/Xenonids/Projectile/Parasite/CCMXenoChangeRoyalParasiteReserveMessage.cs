using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

[Serializable, NetSerializable]
public sealed class CCMXenoChangeRoyalParasiteReserveMessage : BoundUserInterfaceMessage
{
    public int NewRoyalReserve;

    public CCMXenoChangeRoyalParasiteReserveMessage(int newRoyalReserve)
    {
        NewRoyalReserve = newRoyalReserve;
    }
}
