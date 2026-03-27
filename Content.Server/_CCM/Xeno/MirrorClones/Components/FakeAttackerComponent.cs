using Robust.Shared.Audio;

namespace Content.Server._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
public sealed partial class FakeAttackerComponent : Component
{
    public float Accumulator = 0f;

    [DataField] public float AttackInterval = 0.8f;

    [DataField] public float SearchRange = 2.25f;

    [DataField] public SoundSpecifier? SwingSound;
}
