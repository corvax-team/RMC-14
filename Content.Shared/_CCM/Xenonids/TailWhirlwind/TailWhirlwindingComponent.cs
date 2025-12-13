using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.TailWhirlwind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TailWhirlwindSystem))]
public sealed partial class TailWhirlwindingComponent : Component
{
    [DataField, AutoNetworkedField]
    public Angle LastAngle;
}
