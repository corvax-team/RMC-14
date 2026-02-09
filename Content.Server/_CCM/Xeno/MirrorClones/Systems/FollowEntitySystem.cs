using System.Numerics;
using Content.Server._CCM.Xeno.MirrorClones.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class FollowEntitySystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FollowEntityComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var follow, out var xform))
        {
            if (!EntityManager.EntityExists(follow.Target))
                continue;

            if (!TryComp<TransformComponent>(follow.Target, out var targetXform))
                continue;

            var targetPos = targetXform.MapPosition;
            var ourPos = xform.MapPosition;

            if (targetPos.MapId != ourPos.MapId)
            {
                xform.Coordinates = targetXform.Coordinates;
                continue;
            }

            var desired = targetPos.Position + follow.Offset;
            var current = ourPos.Position;

            var delta = desired - current;
            var dist = delta.Length();

            if (dist > follow.TeleportDistance)
            {
                xform.WorldPosition = desired;
                continue;
            }

            var t = MathF.Min(1f, frameTime * follow.FollowStrength);
            var newPos = current + delta * t;

            xform.WorldPosition = newPos;

            xform.LocalRotation = targetXform.LocalRotation;
        }
    }
}
