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
public sealed class VehicleFabricatorBuiState(
    string? PrintingName,
    TimeSpan? PrintAt,
    TimeSpan PrintDelay,
    List<VehicleFabricatorPrintableEntry> Printables,
    Dictionary<string, int> PrintedModules,
    Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>> AvailableCategories) : BoundUserInterfaceState
{
    public readonly string? PrintingName = PrintingName;
    public readonly TimeSpan? PrintAt = PrintAt;
    public readonly TimeSpan PrintDelay = PrintDelay;
    public readonly List<VehicleFabricatorPrintableEntry> Printables = Printables;
    public readonly Dictionary<string, int> PrintedModules = PrintedModules;
    public readonly Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>> AvailableCategories = AvailableCategories;
}

[Serializable, NetSerializable]
public sealed record VehicleFabricatorPrintableEntry(
    EntProtoId Id,
    string Name,
    string Description,
    VehicleFabricatorCategory Category,
    VehicleType Vehicle
);
