using Content.Shared._CCM.Miners.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Miners.Events;

[Serializable, NetSerializable]
public sealed partial class MinerModuleInstallDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public MinerModuleType ModuleType;

    private MinerModuleInstallDoAfterEvent()
    {
    }

    public MinerModuleInstallDoAfterEvent(MinerModuleType moduleType)
    {
        ModuleType = moduleType;
    }
}
