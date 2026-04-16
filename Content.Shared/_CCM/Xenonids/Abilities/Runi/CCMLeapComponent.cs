using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xeno.Abilities.ZigZagPounce;

[RegisterComponent, NetworkedComponent]
public sealed partial class CCMZigZagPounceComponent : Component
{
    [DataField] public float Strength = 45f;
    [DataField] public float MaxDistance = 6f;
    [DataField] public float ZigZagAmplitude = 0.35f;
    [DataField] public float ZigZagFrequency = 10f;
}