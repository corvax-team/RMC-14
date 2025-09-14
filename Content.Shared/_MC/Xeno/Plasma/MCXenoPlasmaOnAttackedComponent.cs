using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPlasmaOnAttackedComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 Multiplier = 1f;
}
