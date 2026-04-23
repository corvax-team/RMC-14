using Content.Shared._CCM.Miners.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Miners.Events;

[Serializable, NetSerializable]
public sealed partial class MinerRepairDoAfterEvent : DoAfterEvent
{
    [DataField]
    public MinerState State;

    private MinerRepairDoAfterEvent()
    {
    }

    public MinerRepairDoAfterEvent(MinerState state)
    {
        State = state;
    }

    public override DoAfterEvent Clone()
    {
        return new MinerRepairDoAfterEvent(State);
    }
}
