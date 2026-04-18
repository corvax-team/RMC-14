using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Actions;

/// <summary>
/// CCM: When present on a controlled entity, indicates that its HUD should also display actions
/// of another source entity (e.g., the pilot while controlling a vehicle), and clicks should
/// be proxied back to that source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMActionsDisplayRelayComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The entity whose actions should be displayed alongside the local entity's actions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Source;

    /// <summary>
    /// If true and the requested action belongs to <see cref="Source"/>, the action will execute
    /// as if it was initiated by <see cref="Source"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool InteractAsSource;
}
