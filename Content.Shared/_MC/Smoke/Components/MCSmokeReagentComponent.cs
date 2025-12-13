using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSmokeReagentComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public List<ReagentQuantity> Reagents = new();
}
