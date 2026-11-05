using Content.Shared._RMC14.Xenonids;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Rejuvenate;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Silencer;

public sealed class CCMFalseHiveSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private readonly HashSet<Entity<XenoComponent>> _xenos = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMFalseHiveCloudComponent, MapInitEvent>(OnCloudMapInit);
        SubscribeLocalEvent<CCMFalseHiveCloudComponent, ComponentShutdown>(OnCloudShutdown);

        SubscribeLocalEvent<CCMFalseHiveSlowedComponent, RefreshMovementSpeedModifiersEvent>(OnSlowedRefreshSpeed);
        SubscribeLocalEvent<CCMFalseHiveSlowedComponent, AfterAutoHandleStateEvent>(OnSlowedHandleState);
        SubscribeLocalEvent<CCMFalseHiveSlowedComponent, ComponentStartup>(OnSlowedStartup);
        SubscribeLocalEvent<CCMFalseHiveSlowedComponent, ComponentRemove>(OnSlowedRemove);
        SubscribeLocalEvent<CCMFalseHiveSlowedComponent, RejuvenateEvent>(OnSlowedRejuvenate);
    }

    private void OnCloudMapInit(Entity<CCMFalseHiveCloudComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        var xform = Transform(ent);
        // Off-grid clouds fall back to a distance check in Update.
        if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? grid))
            return;

        ent.Comp.Grid = gridUid;
        ent.Comp.Tiles.Clear();

        var center = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var radius = Math.Max(ent.Comp.TileRadius, 0);

        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var tile = center + new Vector2i(x, y);

                if ((x != 0 || y != 0) &&
                    _turf.IsTileBlocked(gridUid, tile, CollisionGroup.Impassable, grid))
                {
                    continue;
                }

                ent.Comp.Tiles.Add(tile);

                if (ent.Comp.VisualProto is { } visual)
                    ent.Comp.Visuals.Add(Spawn(visual, _map.GridTileToLocal(gridUid, grid, tile)));
            }
        }
    }

    private void OnCloudShutdown(Entity<CCMFalseHiveCloudComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        foreach (var visual in ent.Comp.Visuals)
        {
            if (!TerminatingOrDeleted(visual))
                QueueDel(visual);
        }

        ent.Comp.Visuals.Clear();
    }

    private void OnSlowedRefreshSpeed(Entity<CCMFalseHiveSlowedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var multiplier = Math.Clamp(ent.Comp.SpeedMultiplier, 0.1f, 1f);
        args.ModifySpeed(multiplier, multiplier);
    }

    private void OnSlowedHandleState(Entity<CCMFalseHiveSlowedComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnSlowedStartup(Entity<CCMFalseHiveSlowedComponent> ent, ref ComponentStartup args)
    {
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnSlowedRemove(Entity<CCMFalseHiveSlowedComponent> ent, ref ComponentRemove args)
    {
        if (!TerminatingOrDeleted(ent))
            _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnSlowedRejuvenate(Entity<CCMFalseHiveSlowedComponent> ent, ref RejuvenateEvent args)
    {
        RemCompDeferred<CCMFalseHiveSlowedComponent>(ent);
    }

    private void ApplySlow(EntityUid xeno, float multiplier, TimeSpan expiresAt)
    {
        if (TryComp(xeno, out CCMFalseHiveSlowedComponent? slowed))
        {
            var changed = false;

            if (multiplier < slowed.SpeedMultiplier)
            {
                slowed.SpeedMultiplier = multiplier;
                changed = true;
            }

            if (expiresAt > slowed.ExpiresAt)
            {
                slowed.ExpiresAt = expiresAt;
                changed = true;
            }

            if (changed)
            {
                Dirty(xeno, slowed);
                _speed.RefreshMovementSpeedModifiers(xeno);
            }

            return;
        }

        slowed = new CCMFalseHiveSlowedComponent
        {
            SpeedMultiplier = multiplier,
            ExpiresAt = expiresAt,
        };
        AddComp(xeno, slowed);
        Dirty(xeno, slowed);
    }

    private bool IsInCloud(CCMFalseHiveCloudComponent cloud, MapCoordinates center, EntityUid xeno)
    {
        var xenoPos = _transform.GetMapCoordinates(xeno);
        if (xenoPos.MapId != center.MapId)
            return false;

        if (cloud.Grid is { } gridUid && TryComp(gridUid, out MapGridComponent? grid))
            return cloud.Tiles.Contains(_map.TileIndicesFor(gridUid, grid, xenoPos));

        var half = cloud.TileRadius + 0.5f;
        return Math.Abs(xenoPos.Position.X - center.Position.X) <= half &&
               Math.Abs(xenoPos.Position.Y - center.Position.Y) <= half;
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var now = _timing.CurTime;

        var clouds = EntityQueryEnumerator<CCMFalseHiveCloudComponent, TransformComponent>();
        while (clouds.MoveNext(out var uid, out var cloud, out var xform))
        {
            if (now < cloud.NextUpdate)
                continue;

            cloud.NextUpdate = now + cloud.UpdateInterval;

            var center = _transform.GetMapCoordinates(uid, xform);

            _xenos.Clear();
            _lookup.GetEntitiesInRange(center, (cloud.TileRadius + 1) * 1.5f, _xenos);

            foreach (var xeno in _xenos)
            {
                if (_mobState.IsDead(xeno))
                    continue;

                if (!IsInCloud(cloud, center, xeno))
                    continue;

                ApplySlow(xeno, cloud.SpeedMultiplier, now + cloud.Linger);
            }
        }

        var slowedQuery = EntityQueryEnumerator<CCMFalseHiveSlowedComponent>();
        while (slowedQuery.MoveNext(out var uid, out var slowed))
        {
            if (slowed.ExpiresAt <= now)
                RemCompDeferred<CCMFalseHiveSlowedComponent>(uid);
        }
    }
}
