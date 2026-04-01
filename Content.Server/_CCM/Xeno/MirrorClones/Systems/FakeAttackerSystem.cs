using Content.Server._CCM.Xeno.MirrorClones.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class FakeAttackerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FakeAttackerComponent, MirrorCloneComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var fake, out var clone, out var xform))
        {
            if (!EntityManager.EntityExists(clone.Original))
                continue;

            fake.Accumulator += frameTime;
            if (fake.Accumulator < fake.AttackInterval)
                continue;

            fake.Accumulator = 0f;

            if (!TryComp<TransformComponent>(clone.Original, out var origXform))
                continue;

            var origin = origXform.MapPosition;
            var target = FindNearestTarget(origin, fake.SearchRange, exclude: uid, exclude2: clone.Original);

            if (target == null)
                continue;

            if (TryComp<TransformComponent>(target.Value, out var targetXform))
            {
                var dir = targetXform.MapPosition.Position - xform.MapPosition.Position;
                if (dir.LengthSquared() > 0.001f)
                    xform.LocalRotation = dir.ToAngle();
            }

            if (fake.SwingSound != null)
                _audio.PlayPvs(fake.SwingSound, uid);
        }
    }

    private EntityUid? FindNearestTarget(MapCoordinates origin, float range, EntityUid exclude, EntityUid exclude2)
    {
        var rangeSqr = range * range;
        EntityUid? best = null;
        var bestDist = float.MaxValue;

        var query = EntityQueryEnumerator<TransformComponent>();

        while (query.MoveNext(out var uid, out var xform))
        {
            if (uid == exclude || uid == exclude2)
                continue;

            if (HasComp<MirrorCloneComponent>(uid))
                continue;

            var mp = xform.MapPosition;
            if (mp.MapId != origin.MapId)
                continue;

            if (!HasComp<PhysicsComponent>(uid))
                continue;

            var d = (mp.Position - origin.Position).LengthSquared();
            if (d > rangeSqr)
                continue;

            if (d < bestDist)
            {
                bestDist = d;
                best = uid;
            }
        }

        return best;
    }
}
