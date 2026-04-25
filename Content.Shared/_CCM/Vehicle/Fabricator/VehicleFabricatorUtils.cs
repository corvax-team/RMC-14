using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Vehicle.Fabricator;

public static class VehicleFabricatorUtils
{
    private const string ProtoTank = "VehicleTank";
    private const string ProtoAPC = "VehicleAPC";
    private const string ProtoHumvee = "VehicleHumvee";
    private const string ProtoVan = "VehicleVan";

    public static string GetLimitKey(VehicleFabricatorCategory category, VehicleType vehicle)
    {
        var categoryStr = category.ToString();
        var vehicleStr = vehicle == VehicleType.None ? "None" : vehicle.ToString();
        return $"{categoryStr}-{vehicleStr}";
    }

    public static EntProtoId? GetVehicleProtoId(VehicleType vehicle) => vehicle switch
    {
        VehicleType.Tank => ProtoTank,
        VehicleType.APC => ProtoAPC,
        VehicleType.Humvee => ProtoHumvee,
        VehicleType.Van => ProtoVan,
        _ => null
    };

    public static VehicleFabricatorCategory GetCategoryFromHardpointType(string hardpointType)
    {
        return hardpointType switch
        {
            "wheel" => VehicleFabricatorCategory.Chassis,
            "turret" => VehicleFabricatorCategory.Primary,
            "secondary" => VehicleFabricatorCategory.Secondary,
            "cannon" => VehicleFabricatorCategory.Cannon,
            "launcher" => VehicleFabricatorCategory.Launcher,
            "armor" => VehicleFabricatorCategory.Armor,
            "support" => VehicleFabricatorCategory.Support,
            "supportattachment" => VehicleFabricatorCategory.Support,
            "roofattachment" => VehicleFabricatorCategory.RoofAttachment,
            "frontattachment" => VehicleFabricatorCategory.FrontAttachment,
            _ => VehicleFabricatorCategory.Support
        };
    }
}
