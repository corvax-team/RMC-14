namespace Content.Shared._CCM.Vehicle;

[RegisterComponent]
public sealed partial class VehicleEnterPointComponent : Component
{
    [DataField]
    public EntryDirection Direction = EntryDirection.Back;
}
