using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

//using Content.Server._RMC14.Xenonids.SummonXeno;

namespace Content.Shared._RMC14.Xenonids.SummonXeno;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSummonXenoSystem))]
public sealed partial class SummonXenoComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 300;

    [DataField, AutoNetworkedField]
    public int Number = 3;

    [DataField, AutoNetworkedField]
    public string EntitieID = "XenoEgg";
}