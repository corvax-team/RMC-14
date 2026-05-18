using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Construction;

public sealed class RMCStructureBuiltEvent : EntityEventArgs
{
    public EntityUid User { get; }
    public int Count { get; }

    public RMCStructureBuiltEvent(EntityUid user, int count = 1)
    {
        User = user;
        Count = count;
    }
}
