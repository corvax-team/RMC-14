using Robust.Shared.GameStates;

namespace Content.Shared._TGMC14.BluespaceBarrier;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BluespaceBarrierSystem))]
public sealed partial class BluespaceBarrierComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan? DisappearTime/* = TimeSpan.FromMinutes(20);*/;

    [DataField, AutoNetworkedField]
    public TimeSpan? DisappearAt;
}
