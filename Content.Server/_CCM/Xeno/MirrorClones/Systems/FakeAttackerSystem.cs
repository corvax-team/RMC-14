using Content.Server._CCM.Xeno.MirrorClones.Components;
using Content.Shared._CCM.Xeno.MirrorClones.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class FakeAttackerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<FakeAttackerComponent, MirrorCloneComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var fake, out var clone, out var xform))
        {
            if (!EntityManager.EntityExists(clone.Original))
                continue;

            fake.Accumulator += frameTime;
            if (fake.Accumulator < fake.AttackInterval)
                continue;

            if (!TryComp(clone.Original, out TransformComponent? origXform))
                continue;

            var origin = _transform.GetMapCoordinates(clone.Original, origXform);
            var target = FindNearestTarget(origin, fake.SearchRange, exclude: uid, exclude2: clone.Original);

            if (target == null)
                continue;

            fake.Accumulator = 0f;

            if (TryComp(target.Value, out TransformComponent? targetXform))
            {
                var targetMapPos = _transform.GetMapCoordinates(target.Value, targetXform);
                var ourMapPos = _transform.GetMapCoordinates(uid, xform);
                var dir = targetMapPos.Position - ourMapPos.Position;
                if (dir.LengthSquared() > 0.001f)
                    _transform.SetLocalRotation(uid, dir.ToAngle(), xform);
            }

            if (fake.SwingSound != null)
                _audio.PlayPvs(fake.SwingSound, uid);

            var swing = EnsureComp<MirrorCloneSwingComponent>(uid);
            swing.Time = 0f;
            swing.Duration = 0.14f;
            swing.LungeDistance = 0.16f;
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

            var mp = _transform.GetMapCoordinates(uid, xform);
            if (mp.MapId != origin.MapId)
                continue;

            if (!HasComp<PhysicsComponent>(uid))
                continue;

            var d = (mp.Position - origin.Position).LengthSquared();
            if (d > rangeSqr)
                continue;

            if (!(d < bestDist)) continue;
            bestDist = d;
            best = uid;
        }

        return best;
    }
}
