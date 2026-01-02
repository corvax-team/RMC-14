using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.TailVortex;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TailVortexSystem))]
public sealed partial class TailVortexRotationComponent : Component
{
    [DataField, AutoNetworkedField]
    public Angle? Angle;
}
