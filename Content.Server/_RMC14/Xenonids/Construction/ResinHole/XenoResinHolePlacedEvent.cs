using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._RMC14.Xenonids.Construction.ResinHole;

public sealed class XenoResinHolePlacedEvent : EntityEventArgs
{
    public EntityUid User { get; }
    public EntProtoId Prototype { get; }

    public XenoResinHolePlacedEvent(EntityUid user, EntProtoId prototype)
    {
        User = user;
        Prototype = prototype;
    }
}
