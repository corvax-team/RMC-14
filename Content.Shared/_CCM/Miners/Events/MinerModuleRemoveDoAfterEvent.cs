using Content.Shared._CCM.Miners.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Miners.Events;

[Serializable, NetSerializable]
public sealed partial class MinerModuleRemoveDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public MinerModuleType ModuleType;

    private MinerModuleRemoveDoAfterEvent()
    {
    }

    public MinerModuleRemoveDoAfterEvent(MinerModuleType moduleType)
    {
        ModuleType = moduleType;
    }
}
