using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSmokeComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EffectDelay = TimeSpan.FromSeconds(1);

    [AutoNetworkedField]
    public TimeSpan EffectNext;

    [AutoNetworkedField]
    public List<EntityUid> AffectedEntities = new();
}
