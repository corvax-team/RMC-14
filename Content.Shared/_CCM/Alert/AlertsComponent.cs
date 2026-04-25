using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Alert;

/// <summary>
/// When present on a controlled entity, indicates that its HUD should display alerts
/// of another source entity (e.g., the pilot while controlling a vehicle), and clicks should
/// be proxied back to that source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMAlertsDisplayRelayComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The entity whose alerts should be displayed is the local entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Source;

    /// <summary>
    /// If true and this entity is displaying alerts for <see cref="Source"/>, clicking alerts will activate them
    /// as if the click originated from <see cref="Source"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool InteractAsSource;
}
