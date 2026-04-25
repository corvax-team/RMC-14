using Content.Server._CCM.Xeno.MirrorClones.Components;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class MirrorCloneMimicAttackSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string ExtraDamageTypeId = "Cellular";
    private readonly Dictionary<EntityUid, TimeSpan> _recentExtra = new();
    private static readonly TimeSpan RecursionWindow = TimeSpan.FromMilliseconds(80);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHitVisual);

        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnMeleeHitVisual(Entity<MeleeWeaponComponent> attacker, ref MeleeHitEvent args)
    {
        MimicClonesSwing(attacker.Owner);
    }

    private void OnDamageChanged(EntityUid uid, DamageableComponent comp, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        if (args.DamageDelta == null || args.DamageDelta.Empty)
            return;

        var hasPositive = false;
        foreach (var v in args.DamageDelta.DamageDict.Values)
        {
            if (v > FixedPoint2.Zero)
            {
                hasPositive = true;
                break;
            }
        }
        if (!hasPositive)
            return;

        var now = _timing.CurTime;
        if (_recentExtra.TryGetValue(uid, out var last) && now - last < RecursionWindow)
        {
            return;
        }

        var origin = args.Origin;
        if (origin == null || !origin.Value.IsValid())
            return;

        var originIsXeno = HasComp<XenoComponent>(origin.Value);
        if (!originIsXeno)
            return;

        if (!TryComp(origin.Value, out MirrorClonesActiveComponent? originActive))
            return;

        if (originActive.TimeLeft < 0f)
            return;

        _recentExtra[uid] = now;

        var extra = new DamageSpecifier();
        extra.DamageDict[ExtraDamageTypeId] = FixedPoint2.New(originActive.GeneticDamage); // 10

        _damageable.TryChangeDamage(uid, extra, origin: origin.Value);
    }

    private void MimicClonesSwing(EntityUid original)
    {
        var clones = EntityQueryEnumerator<MirrorCloneComponent>();
        while (clones.MoveNext(out var cloneUid, out var mirror))
        {
            if (mirror.Original != original)
                continue;

            var swing = EnsureComp<MirrorCloneSwingComponent>(cloneUid);
            swing.Time = 0f;
            swing.Duration = 0.14f;
            swing.LungeDistance = 0.16f;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var now = _timing.CurTime;
        List<EntityUid>? list = null;

        foreach (var (ent, time) in _recentExtra)
        {
            if (now - time > TimeSpan.FromSeconds(2))
            {
                list ??= new List<EntityUid>();
                list.Add(ent);
            }
        }

        if (list != null)
        {
            foreach (var ent in list)
            {
                _recentExtra.Remove(ent);
            }
        }
    }
}