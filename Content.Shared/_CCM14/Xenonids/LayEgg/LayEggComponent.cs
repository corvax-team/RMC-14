using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._RMC14.Xenonids.LayEgg;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedLayEggSystem))]
public sealed partial class LayEggComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 800;

    [DataField, AutoNetworkedField]
    public string EntitieID = "XenoEgg";
}