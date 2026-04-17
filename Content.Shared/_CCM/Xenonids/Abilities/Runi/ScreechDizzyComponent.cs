using Robust.Shared.GameStates;

namespace Content.Shared._CCM14.Xenonids.Screech;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ScreechDizzyComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    [DataField, AutoNetworkedField]
    public EntityUid Source; 
}