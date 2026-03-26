using Content.Server._CCM.Xeno.MirrorClones.Components;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class FakeAttackerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedRMCMeleeWeaponSystem _rmcMelee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

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

            var target = FindNearestTarget(uid, fake.SearchRange, clone.Original);

            if (target == null)
                continue;

            fake.Accumulator = 0f;

            if (!TryComp<TransformComponent>(target.Value, out var targetXform))
                continue;

            var dir = _transform.GetWorldPosition(target.Value) - _transform.GetWorldPosition(uid);

            if (dir.LengthSquared() > 0.01f)
                xform.LocalRotation = dir.ToAngle();

            if (!(dir.LengthSquared() <= 2.25f)) continue;
            _rmcMelee.DoLunge(uid, target.Value);

            if (fake.SwingSound != null)
                _audio.PlayPvs(fake.SwingSound, uid);
        }
    }

    private EntityUid? FindNearestTarget(EntityUid clone, float range, EntityUid original)
    {
        var xform = Transform(clone);
        var rangeSqr = range * range;
        EntityUid? best = null;
        var bestDist = rangeSqr;

        foreach (var ent in _lookup.GetEntitiesInRange(xform.MapPosition, range))
        {
            if (ent == clone || ent == original || HasComp<MirrorCloneComponent>(ent))
                continue;

            if (HasComp<XenoComponent>(ent))
                continue;

            if (!HasComp<PhysicsComponent>(ent))
                continue;

            var distSqr = (_transform.GetWorldPosition(ent) - _transform.GetWorldPosition(clone)).LengthSquared();
            if (!(distSqr < bestDist)) continue;
            bestDist = distSqr;
            best = ent;
        }

        return best;
    }
}
