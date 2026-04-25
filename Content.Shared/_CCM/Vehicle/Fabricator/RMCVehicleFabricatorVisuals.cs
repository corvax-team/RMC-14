using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[Serializable]
[NetSerializable]
public enum RMCVehicleFabricatorVisuals
{
    State
}

[Serializable]
[NetSerializable]
public enum RMCVehicleFabricatorState
{
    Idle,
    Fabricating
}