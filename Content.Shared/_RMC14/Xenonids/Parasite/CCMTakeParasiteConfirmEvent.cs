using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[Serializable, NetSerializable]
public record struct CCMTakeParasiteConfirmEvent(NetEntity ParasiteUid, NetEntity ActorUid, bool IsRoyal) : IEntityEventSubscriber;

[Serializable, NetSerializable]
public record struct CCMTakeCarrierParasiteConfirmEvent(NetEntity CarrierUid, NetEntity ActorUid, bool IsRoyal) : IEntityEventSubscriber;
