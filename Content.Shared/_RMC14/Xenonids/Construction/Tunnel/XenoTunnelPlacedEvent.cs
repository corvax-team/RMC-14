using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Construction.Tunnel;

public sealed class XenoTunnelPlacedEvent : EntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Tunnel { get; }

    public XenoTunnelPlacedEvent(EntityUid user, EntityUid tunnel)
    {
        User = user;
        Tunnel = tunnel;
    }
}
