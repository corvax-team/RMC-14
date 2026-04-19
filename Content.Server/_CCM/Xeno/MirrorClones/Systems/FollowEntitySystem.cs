using Content.Server._CCM.Xeno.MirrorClones.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class FollowEntitySystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<FollowEntityComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var follow, out var xform))
        {
            if (!EntityManager.EntityExists(follow.Target))
                continue;

            if (!TryComp(follow.Target, out TransformComponent? targetXform))
                continue;

            var targetPos = _transform.GetMapCoordinates(follow.Target, targetXform);
            var ourPos = _transform.GetMapCoordinates(uid, xform);

            if (targetPos.MapId != ourPos.MapId)
            {
                _transform.SetWorldPosition(uid, targetPos.Position);
                continue;
            }

            var desired = targetPos.Position + follow.Offset;
            var current = ourPos.Position;

            var delta = desired - current;
            var dist = delta.Length();

            if (dist > follow.TeleportDistance)
            {
                _transform.SetWorldPosition(uid, desired);
                continue;
            }

            var t = MathF.Min(1f, frameTime * follow.FollowStrength);
            var newPos = current + delta * t;

            _transform.SetWorldPosition(uid, newPos);

            if (follow.RotateWithTarget)
                _transform.SetLocalRotation(uid, targetXform.LocalRotation);
        }
    }
}
