using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma.Components.Conversions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPlasmaOnAttackedComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Multiplier = 1f;
}
