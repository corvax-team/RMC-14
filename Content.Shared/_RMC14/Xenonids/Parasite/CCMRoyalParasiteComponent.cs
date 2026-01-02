using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedXenoParasiteSystem))]
public sealed partial class CCMRoyalParasiteComponent : Component
{
    [DataField, AutoNetworkedField]
    public int InfectionCount = 0;

    [DataField, AutoNetworkedField]
    public int MaxInfections = 2;

    [DataField]
    public TimeSpan InfectionCooldown = TimeSpan.FromSeconds(30);

    [AutoNetworkedField]
    public TimeSpan NextInfectionTime;
}
