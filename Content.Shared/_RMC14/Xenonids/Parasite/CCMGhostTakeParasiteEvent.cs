using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[Serializable, NetSerializable]
public sealed class CCMGhostTakeParasiteEvent : EntityEventArgs
{
    public NetEntity ParasiteUid { get; }
    public bool IsRoyal { get; }

    public CCMGhostTakeParasiteEvent(NetEntity parasiteUid, bool isRoyal = false)
    {
        ParasiteUid = parasiteUid;
        IsRoyal = isRoyal;
    }
}

[Serializable, NetSerializable]
public sealed class CCMGhostTakeCarrierParasiteEvent : EntityEventArgs
{
    public NetEntity CarrierUid { get; }
    public bool IsRoyal { get; }

    public CCMGhostTakeCarrierParasiteEvent(NetEntity carrierUid, bool isRoyal = false)
    {
        CarrierUid = carrierUid;
        IsRoyal = isRoyal;
    }
}
