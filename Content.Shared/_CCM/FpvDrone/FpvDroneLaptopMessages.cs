using Robust.Shared.Serialization;

namespace Content.Shared._CCM.FpvDrone;

[Serializable, NetSerializable]
public sealed record FpvDroneLaptopInfo(
    NetEntity Id,
    string Name,
    string Role,
    float Health,
    float MaxHealth,
    bool Connected,
    bool SignalLost,
    bool IsControlled,
    string? OperatorName,
    bool CanDetonate
);

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopBuiState(List<FpvDroneLaptopInfo> drones) : BoundUserInterfaceState
{
    public readonly List<FpvDroneLaptopInfo> Drones = drones;
}

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopSelectDroneBuiMsg(NetEntity drone) : BoundUserInterfaceMessage
{
    public NetEntity Drone = drone;
}

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopToggleControlBuiMsg : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopDetonateBuiMsg(NetEntity drone) : BoundUserInterfaceMessage
{
    public NetEntity Drone = drone;
}

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopUnlinkBuiMsg(NetEntity drone) : BoundUserInterfaceMessage
{
    public NetEntity Drone = drone;
}

[Serializable, NetSerializable]
public sealed class FpvDroneLaptopUnlinkAllBuiMsg : BoundUserInterfaceMessage;
