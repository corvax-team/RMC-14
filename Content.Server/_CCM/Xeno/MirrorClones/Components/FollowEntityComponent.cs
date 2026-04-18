using System.Numerics;

namespace Content.Server._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
public sealed partial class FollowEntityComponent : Component
{
    public EntityUid Target;

    [DataField] public float FollowStrength = 10f;
    [DataField] public float TeleportDistance = 2.5f;

    [DataField] public Vector2 Offset;

    [DataField] public bool RotateWithTarget = true;
}
