using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Silencer;

public sealed class CCMSonicCarbineSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<XenoComponent> _xenoQuery;
    private EntityQuery<RMCSizeComponent> _sizeQuery;

    public override void Initialize()
    {
        _xenoQuery = GetEntityQuery<XenoComponent>();
        _sizeQuery = GetEntityQuery<RMCSizeComponent>();

        SubscribeLocalEvent<CCMSonicRoundComponent, ProjectileHitEvent>(OnRoundHit);

        SubscribeLocalEvent<CCMSonicStacksComponent, RefreshMovementSpeedModifiersEvent>(OnStacksRefreshSpeed);
        SubscribeLocalEvent<CCMSonicStacksComponent, AfterAutoHandleStateEvent>(OnStacksHandleState);
        SubscribeLocalEvent<CCMSonicStacksComponent, ComponentRemove>(OnStacksRemove);
        SubscribeLocalEvent<CCMSonicStacksComponent, RejuvenateEvent>(OnStacksRejuvenate);

        SubscribeLocalEvent<CCMHiveDisruptedComponent, RejuvenateEvent>(OnDisruptedRejuvenate);
    }

    public CCMSonicXenoClass GetXenoClass(EntityUid xeno)
    {
        if (!_xenoQuery.TryComp(xeno, out var comp))
            return CCMSonicXenoClass.Small;

        if (comp.Tier >= 3)
            return CCMSonicXenoClass.Large;

        if (comp.Tier == 2)
            return CCMSonicXenoClass.Medium;

        if (comp.Tier <= 0 &&
            _sizeQuery.TryComp(xeno, out var size) &&
            size.Size >= RMCSizes.Big)
        {
            return CCMSonicXenoClass.Large;
        }

        return CCMSonicXenoClass.Small;
    }

    public bool IsHiveDisrupted(EntityUid uid)
    {
        return TryComp(uid, out CCMHiveDisruptedComponent? disrupted) && disrupted.ExpiresAt > _timing.CurTime;
    }

    public void DisruptHive(EntityUid xeno, TimeSpan duration)
    {
        var expires = _timing.CurTime + duration;
        var isNew = !HasComp<CCMHiveDisruptedComponent>(xeno);
        var disrupted = EnsureComp<CCMHiveDisruptedComponent>(xeno);
        if (disrupted.ExpiresAt < expires)
        {
            disrupted.ExpiresAt = expires;
            Dirty(xeno, disrupted);
        }

        if (isNew)
            _popup.PopupEntity(Loc.GetString("ccm-silencer-hive-disrupted"), xeno, xeno, PopupType.MediumCaution);
    }

    public void AddSonicStack(EntityUid xeno, float slowPerStack, int maxStacks, TimeSpan duration)
    {
        if (maxStacks <= 0)
            return;

        var stacks = EnsureComp<CCMSonicStacksComponent>(xeno);
        var expires = _timing.CurTime + duration;

        if (stacks.Stacks.Count >= maxStacks)
        {
            var oldest = 0;
            for (var i = 1; i < stacks.Stacks.Count; i++)
            {
                if (stacks.Stacks[i] < stacks.Stacks[oldest])
                    oldest = i;
            }

            stacks.Stacks[oldest] = expires;
        }
        else
        {
            stacks.Stacks.Add(expires);
        }

        stacks.SlowPerStack = slowPerStack;
        Dirty(xeno, stacks);
        _speed.RefreshMovementSpeedModifiers(xeno);
    }

    private void OnRoundHit(Entity<CCMSonicRoundComponent> ent, ref ProjectileHitEvent args)
    {
        if (_net.IsClient)
            return;

        var target = args.Target;
        if (!_xenoQuery.HasComp(target) || _mobState.IsDead(target))
            return;

        var slow = GetXenoClass(target) == CCMSonicXenoClass.Large
            ? ent.Comp.SlowPerStackLarge
            : ent.Comp.SlowPerStackSmall;

        AddSonicStack(target, slow, ent.Comp.MaxStacks, ent.Comp.StackDuration);
        DisruptHive(target, ent.Comp.HiveDisruptDuration);
    }

    private void OnStacksRefreshSpeed(Entity<CCMSonicStacksComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var count = ent.Comp.Stacks.Count;
        if (count <= 0)
            return;

        var multiplier = Math.Clamp(1f - ent.Comp.SlowPerStack * count, 0.1f, 1f);
        args.ModifySpeed(multiplier, multiplier);
    }

    private void OnStacksHandleState(Entity<CCMSonicStacksComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnStacksRemove(Entity<CCMSonicStacksComponent> ent, ref ComponentRemove args)
    {
        if (!TerminatingOrDeleted(ent))
            _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnStacksRejuvenate(Entity<CCMSonicStacksComponent> ent, ref RejuvenateEvent args)
    {
        RemCompDeferred<CCMSonicStacksComponent>(ent);
    }

    private void OnDisruptedRejuvenate(Entity<CCMHiveDisruptedComponent> ent, ref RejuvenateEvent args)
    {
        RemCompDeferred<CCMHiveDisruptedComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var now = _timing.CurTime;

        var stacksQuery = EntityQueryEnumerator<CCMSonicStacksComponent>();
        while (stacksQuery.MoveNext(out var uid, out var stacks))
        {
            var removed = stacks.Stacks.RemoveAll(expires => expires <= now);
            if (removed == 0)
                continue;

            if (stacks.Stacks.Count == 0)
            {
                RemCompDeferred<CCMSonicStacksComponent>(uid);
                continue;
            }

            Dirty(uid, stacks);
            _speed.RefreshMovementSpeedModifiers(uid);
        }

        var disruptedQuery = EntityQueryEnumerator<CCMHiveDisruptedComponent>();
        while (disruptedQuery.MoveNext(out var uid, out var disrupted))
        {
            if (disrupted.ExpiresAt > now)
                continue;

            RemCompDeferred<CCMHiveDisruptedComponent>(uid);
            _popup.PopupEntity(Loc.GetString("ccm-silencer-hive-restored"), uid, uid, PopupType.Medium);
        }
    }
}
