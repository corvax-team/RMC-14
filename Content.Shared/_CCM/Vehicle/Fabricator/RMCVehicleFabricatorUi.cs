using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[Serializable]
[NetSerializable]
public enum RMCVehicleFabricatorUi
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class RMCVehicleFabricatorPrintMsg(EntProtoId id) : BoundUserInterfaceMessage
{
    public readonly EntProtoId Id = id;
}

[Serializable]
[NetSerializable]
public sealed record RMCVehicleFabricatorPrintableDisplayData(
    EntProtoId Id,
    string Name,
    string Description,
    int Cost
);