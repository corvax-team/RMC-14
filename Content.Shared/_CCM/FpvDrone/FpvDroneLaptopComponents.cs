using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.FpvDrone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFpvDroneLaptopSystem))]
public sealed partial class FpvDroneLaptopComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsOpen;

    [DataField, AutoNetworkedField]
    public bool IsPowered;

    [DataField]
    public HashSet<EntityUid> LinkedDrones = new();

    [DataField]
    public int MaxLinkedDrones = 6;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFpvDroneLaptopSystem))]
public sealed partial class FpvDroneLaptopLinkedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedLaptop;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFpvDroneLaptopSystem))]
public sealed partial class FpvDroneLaptopWatcherComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Laptop;

    [DataField, AutoNetworkedField]
    public NetEntity? CurrentDrone;

    [DataField, AutoNetworkedField]
    public bool ControlEnabled;
}

[Serializable, NetSerializable]
public enum FpvDroneLaptopUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum FpvDroneLaptopVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum FpvDroneLaptopVisualLayers : byte
{
    Base
}

[Serializable, NetSerializable]
public enum FpvDroneLaptopState : byte
{
    Closed,
    Open,
    Active
}
