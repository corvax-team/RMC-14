namespace Content.Shared._CCM.Vehicle;

[RegisterComponent]
public sealed partial class VehicleInteriorDoorComponent : Component
{
    [DataField]
    public EntryDirection Side = EntryDirection.Back;
}
