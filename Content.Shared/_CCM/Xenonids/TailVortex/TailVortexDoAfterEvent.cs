using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Xenonids.TailVortex;

[Serializable, NetSerializable]
public sealed partial class TailVortexDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public int TurnsQuantity;

    public TailVortexDoAfterEvent(int turnsQuantity)
    {
        TurnsQuantity = turnsQuantity;
    }
}
