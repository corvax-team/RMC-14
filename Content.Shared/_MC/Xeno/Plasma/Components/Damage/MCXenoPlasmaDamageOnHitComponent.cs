using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma.Components.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoPlasmaDamageOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Amount;

    [DataField, AutoNetworkedField]
    public float Multiplier;

    [DataField, AutoNetworkedField]
    public float MissingMultiplier;
}
