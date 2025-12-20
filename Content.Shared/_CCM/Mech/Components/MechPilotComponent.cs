using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Mech.Components;

/// <summary>
/// Attached to entities piloting a <see cref="CCMMechComponent"/>
/// </summary>
/// <remarks>
/// Get in the robot, Shinji
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMMechPilotComponent : Component
{
    /// <summary>
    /// The mech being piloted
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid Mech;
}
