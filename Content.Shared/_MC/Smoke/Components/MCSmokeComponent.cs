using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCSmokeComponent : Component
{
    [DataField]
    public TimeSpan EffectDelay = TimeSpan.FromSeconds(1);

    public TimeSpan EffectNext;

    public HashSet<EntityUid> AffectedEntities = [];
}
