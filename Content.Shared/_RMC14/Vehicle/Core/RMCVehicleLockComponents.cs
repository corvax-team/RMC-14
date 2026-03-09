using System.Collections.Generic;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._RMC14.Vehicle;

[RegisterComponent]
[Access(typeof(RMCVehicleLockSystem), typeof(RMCVehicleSystem))]
public sealed partial class RMCVehicleLockComponent : Component
{
    [DataField]
    public bool Locked;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RMCVehicleLockSystem))]
public sealed partial class RMCVehicleLockActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "ActionRMCVehicleLock";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public EntityUid? Vehicle;

    [DataField]
    public HashSet<EntityUid> Sources = new();
}

public sealed partial class RMCVehicleLockActionEvent : InstantActionEvent;
