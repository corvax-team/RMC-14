using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Xenonids.Slam;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedXenoTailSlamSystem))]
public sealed partial class XenoTailSlamActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan MissCooldown = TimeSpan.FromSeconds(1);
}
