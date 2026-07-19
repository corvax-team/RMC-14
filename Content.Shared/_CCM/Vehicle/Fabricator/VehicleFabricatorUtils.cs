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

    public static VehicleFabricatorCategory GetCategoryFromHardpointType(string? hardpointTypeId)
    {
        if (string.IsNullOrWhiteSpace(hardpointTypeId))
            return VehicleFabricatorCategory.Support;

        var type = hardpointTypeId.Trim();
        if (type.StartsWith("HardpointType", StringComparison.OrdinalIgnoreCase))
            type = type.Substring("HardpointType".Length);
        else if (type.StartsWith("HardpointSlotType", StringComparison.OrdinalIgnoreCase))
            type = type.Substring("HardpointSlotType".Length);

        var key = type.ToLowerInvariant();
        return key switch
        {
            "wheel" or "treads" or "tires" => VehicleFabricatorCategory.Chassis,
            "turret" => VehicleFabricatorCategory.Primary,
            "secondary" => VehicleFabricatorCategory.Secondary,
            "cannon" => VehicleFabricatorCategory.Cannon,
            "launcher" => VehicleFabricatorCategory.Launcher,
            "armor" => VehicleFabricatorCategory.Armor,
            "support" or "supportattachment" => VehicleFabricatorCategory.Support,
            "roofattachment" => VehicleFabricatorCategory.RoofAttachment,
            "frontattachment" => VehicleFabricatorCategory.FrontAttachment,
            _ => VehicleFabricatorCategory.Support
        };
    }
}
