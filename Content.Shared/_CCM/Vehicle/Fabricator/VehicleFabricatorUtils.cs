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
        return $"{category}-{vehicle}";
    }

    public static EntProtoId? GetVehicleProtoId(VehicleType vehicle) => vehicle switch
    {
        VehicleType.Tank => ProtoTank,
        VehicleType.APC => ProtoAPC,
        VehicleType.Humvee => ProtoHumvee,
        VehicleType.Van => ProtoVan,
        _ => null
    };

    public static VehicleFabricatorCategory GetCategoryFromHardpointType(string? hardpointType)
    {
        if (string.IsNullOrWhiteSpace(hardpointType))
            return VehicleFabricatorCategory.Support;

        var type = hardpointType.Trim().ToLowerInvariant();
        
        return type switch
        {
            "wheel" or "chassis" => VehicleFabricatorCategory.Chassis,
            "turret" or "primary" => VehicleFabricatorCategory.Primary,
            "secondary" => VehicleFabricatorCategory.Secondary,
            "cannon" => VehicleFabricatorCategory.Cannon,
            "launcher" => VehicleFabricatorCategory.Launcher,
            "armor" => VehicleFabricatorCategory.Armor,
            "support" or "supportattachment" => VehicleFabricatorCategory.Support,
            "roof" or "roofattachment" => VehicleFabricatorCategory.RoofAttachment,
            "front" or "frontattachment" => VehicleFabricatorCategory.FrontAttachment,
            _ => VehicleFabricatorCategory.Support
        };
    }
}
