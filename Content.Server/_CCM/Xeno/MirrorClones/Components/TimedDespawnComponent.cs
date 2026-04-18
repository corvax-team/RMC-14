namespace Content.Server._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
public sealed partial class CCMTimedDespawnComponent : Component
{
    [DataField]
    public float Lifetime = 10f;

    [DataField]
    public float Accumulator;
}
