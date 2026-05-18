using Content.Server._RMC14.TacticalMap;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared._CCM.Teleporter;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._CCM.Teleporter;

public sealed class CCMPlanetTeleporterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly TacticalMapSystem _tacticalMap = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        _mapGridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        Subs.BuiEvents<CCMPlanetTeleporterComponent>(CCMPlanetTeleporterUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<CCMPlanetTeleporterSelectMsg>(OnSelect);
        });
    }

    public void TryOpenTeleporter(Entity<CCMPlanetTeleporterComponent?> teleporter, EntityUid user)
    {
        if (!Resolve(teleporter, ref teleporter.Comp, false))
            return;

        _ui.TryOpenUi(teleporter.Owner, CCMPlanetTeleporterUiKey.Key, user);
        UpdateUi((teleporter.Owner, teleporter.Comp), user);
    }

    private void OnOpened(Entity<CCMPlanetTeleporterComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent, args.Actor);
    }

    private void UpdateUi(Entity<CCMPlanetTeleporterComponent> ent, EntityUid actor)
    {
        var user = EnsureComp<CCMPlanetTeleporterUserComponent>(actor);
        var now = _timing.CurTime;
        var remaining = user.NextUseAt > now ? user.NextUseAt - now : TimeSpan.Zero;

        if (!TryResolvePlanetMap(ent.Comp.PlanetMapId, out var map, out var name))
        {
            _ui.SetUiState(ent.Owner, CCMPlanetTeleporterUiKey.Key,
                new CCMPlanetTeleporterState(NetEntity.Invalid, null, user.Teleported, remaining));
            return;
        }

        _ui.SetUiState(ent.Owner, CCMPlanetTeleporterUiKey.Key,
            new CCMPlanetTeleporterState(GetNetEntity(map), name, user.Teleported, remaining));
    }

    private bool TryResolvePlanetMap(string mapId, out EntityUid mapEntity, out string? name)
    {
        mapEntity = default;
        name = null;

        // Prefer a tac map with the requested MapId.
        var query = EntityQueryEnumerator<TacticalMapComponent>();
        while (query.MoveNext(out var uid, out var tacMap))
        {
            if (!string.Equals(tacMap.MapId, mapId, StringComparison.OrdinalIgnoreCase))
                continue;

            mapEntity = uid;
            name = tacMap.DisplayName;
            return true;
        }

        // Fallback: any available tac map.
        if (_tacticalMap.TryGetTacticalMap(out var any))
        {
            mapEntity = any.Owner;
            name = any.Comp.DisplayName;
            return true;
        }

        return false;
    }

    private void OnSelect(Entity<CCMPlanetTeleporterComponent> ent, ref CCMPlanetTeleporterSelectMsg args)
    {
        var actor = args.Actor;
        var user = EnsureComp<CCMPlanetTeleporterUserComponent>(actor);
        var now = _timing.CurTime;

        if (user.NextUseAt > now)
        {
            var seconds = (int) Math.Ceiling((user.NextUseAt - now).TotalSeconds);
            _popup.PopupClient(Loc.GetString("ccm-planet-teleporter-cooldown-popup", ("seconds", seconds)), actor, actor, PopupType.SmallCaution);
            UpdateUi(ent, actor);
            return;
        }

        if (!user.Teleported)
        {
            if (!TryResolvePlanetMap(ent.Comp.PlanetMapId, out var mapEntity, out _))
                return;

            if (!_mapGridQuery.TryComp(mapEntity, out var grid))
                return;

            user.Origin = _transform.GetMoverCoordinates(actor);
            user.Teleported = true;
            TeleportToMapIndices(actor, mapEntity, grid.TileSize, args.Position);
        }
        else
        {
            if (user.Origin is { } origin)
            {
                _transform.SetCoordinates(actor, origin);
                _transform.AttachToGridOrMap(actor);
                if (_physicsQuery.TryComp(actor, out var physics))
                    _physics.SetLinearVelocity(actor, Vector2.Zero, body: physics);
            }

            user.Origin = null;
            user.Teleported = false;
            user.NextUseAt = now + TimeSpan.FromSeconds(ent.Comp.CooldownSeconds);
        }

        Dirty(actor, user);
        UpdateUi(ent, actor);
    }

    private void TeleportToMapIndices(EntityUid actor, EntityUid mapUid, float tileSize, Vector2i indices)
    {
        var tileCoords = new Vector2(indices.X, indices.Y);
        var coords = new EntityCoordinates(mapUid, tileCoords * tileSize);
        _transform.SetCoordinates(actor, coords);
        _transform.AttachToGridOrMap(actor);

        if (_physicsQuery.TryComp(actor, out var physics))
            _physics.SetLinearVelocity(actor, Vector2.Zero, body: physics);
    }
}
