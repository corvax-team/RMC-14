using System.Numerics;
using Robust.Shared.Maths;

namespace Content.Server._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
public sealed partial class FollowEntityComponent : Component
{
    public EntityUid Target;

    [DataField] public float FollowStrength = 10f;
    [DataField] public float TeleportDistance = 2.5f;

    [DataField] public Vector2 Offset = Vector2.Zero;

    [DataField] public Vector2 LocalOffset = Vector2.Zero;

    [DataField] public bool RotateWithTarget = true;

    [DataField] public Angle LockedAngle = Angle.Zero;
}
