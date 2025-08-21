using Robust.Shared.GameStates;

namespace Content.Shared._TGMC14.BiologicalBarrier;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BiologicalBarrierSystem))]
public sealed partial class BiologicalBarrierComponent : Component
{
    [DataField, AutoNetworkedField]
    public int? DisappearWhen;
}
