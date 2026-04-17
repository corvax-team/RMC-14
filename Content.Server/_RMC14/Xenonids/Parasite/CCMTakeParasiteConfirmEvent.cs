using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server._RMC14.Xenonids.Parasite;

public record struct TakeParasiteConfirmEvent(NetEntity ParasiteUid, NetEntity ActorUid, bool IsRoyal) : IEntityEventSubscriber;

public record struct TakeCarrierParasiteConfirmEvent(NetEntity CarrierUid, NetEntity ActorUid, bool IsRoyal) : IEntityEventSubscriber;
