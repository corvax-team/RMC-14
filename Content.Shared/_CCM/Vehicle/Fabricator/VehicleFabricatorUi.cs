using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[Serializable]
[NetSerializable]
public enum VehicleFabricatorUi
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class VehicleFabricatorPrintMsg(EntProtoId id) : BoundUserInterfaceMessage
{
    public readonly EntProtoId Id = id;
}

[Serializable]
[NetSerializable]
public sealed record VehicleFabricatorPrintableDisplayData(
    EntProtoId Id,
    string Name,
    string Description,
    int Cost
);