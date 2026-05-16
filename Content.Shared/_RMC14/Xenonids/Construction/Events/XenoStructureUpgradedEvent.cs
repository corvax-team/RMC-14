using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Construction.Events;

public sealed class XenoStructureUpgradedEvent : EntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Structure { get; }

    public XenoStructureUpgradedEvent(EntityUid user, EntityUid structure)
    {
        User = user;
        Structure = structure;
    }
}
