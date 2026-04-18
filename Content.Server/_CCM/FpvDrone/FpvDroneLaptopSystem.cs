using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Shared._CCM.FpvDrone;
using Content.Shared.Damage;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._CCM.FpvDrone;

public sealed class FpvDroneLaptopSystem : SharedFpvDroneLaptopSystem
{
    [Dependency] private readonly FpvDroneSystem _drone = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscribers = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<FpvDroneLaptopWatcherComponent, ComponentShutdown>(OnWatcherShutdown);
        SubscribeLocalEvent<FpvDroneLaptopWatcherComponent, PlayerDetachedEvent>(OnWatcherDetached);
        SubscribeLocalEvent<NewLinkEvent>(OnNewLink);

        Subs.BuiEvents<FpvDroneLaptopComponent>(FpvDroneLaptopUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUiOpened);
            subs.Event<BoundUIClosedEvent>(OnUiClosed);
            subs.Event<FpvDroneLaptopSelectDroneBuiMsg>(OnSelectDrone);
            subs.Event<FpvDroneLaptopToggleControlBuiMsg>(OnToggleControl);
            subs.Event<FpvDroneLaptopDetonateBuiMsg>(OnDetonateDrone);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CleanupWatchers();
    }

    private void OnNewLink(NewLinkEvent args)
    {
        if (!TryComp<FpvDroneLaptopComponent>(args.Source, out var laptop))
            return;

        if (!TryComp<FpvDroneComponent>(args.Sink, out var drone))
            return;

        if (args.SourcePort != "FpvDroneControl")
            return;

        if (TryComp<FpvDroneLaptopLinkedComponent>(args.Sink, out var existingLink) && existingLink.LinkedLaptop != null)
        {
            if (args.User != null)
                _popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-already-linked"), args.Source, args.User.Value);
            return;
        }

        if (laptop.LinkedDrones.Count >= laptop.MaxLinkedDrones)
        {
            if (args.User != null)
                _popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-capacity"), args.Source, args.User.Value);
            return;
        }

        var link = EnsureComp<FpvDroneLaptopLinkedComponent>(args.Sink);
        link.LinkedLaptop = args.Source;
        drone.Control = args.Source;
        laptop.LinkedDrones.Add(args.Sink);

        Dirty(args.Sink, link);
        Dirty(args.Sink, drone);
        Dirty(args.Source, laptop);

        if (args.User != null)
            _popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-linked"), args.Source, args.User.Value);

        UpdateUI((args.Source, laptop));
    }

    private void OnUiOpened(Entity<FpvDroneLaptopComponent> ent, ref BoundUIOpenedEvent args)
    {
        var watcher = EnsureComp<FpvDroneLaptopWatcherComponent>(args.Actor);
        watcher.Laptop = ent.Owner;
        Dirty(args.Actor, watcher);

        UpdateUI(ent);
    }

    private void OnUiClosed(Entity<FpvDroneLaptopComponent> ent, ref BoundUIClosedEvent args)
    {
        if (TryComp<FpvDroneLaptopWatcherComponent>(args.Actor, out var watcher) &&
            watcher.Laptop == ent.Owner)
        {
            ClearWatcher(args.Actor, watcher);
        }
    }

    private void OnSelectDrone(Entity<FpvDroneLaptopComponent> ent, ref FpvDroneLaptopSelectDroneBuiMsg args)
    {
        if (!TryGetEntity(args.Drone, out var droneUid))
            return;

        if (!ent.Comp.LinkedDrones.Contains(droneUid.Value))
            return;

        if (!TryComp<FpvDroneComponent>(droneUid.Value, out var drone) ||
            !_drone.IsControlLinkInRange(ent.Owner, droneUid.Value, drone))
        {
            _popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-no-signal"), ent, args.Actor);
            return;
        }

        var watcher = EnsureComp<FpvDroneLaptopWatcherComponent>(args.Actor);
        watcher.Laptop = ent.Owner;

        if (watcher.ControlEnabled)
            StopRemoteControl(args.Actor, watcher);

        SetWatchedDrone(args.Actor, watcher, ent.Owner, droneUid.Value);
    }

    private void OnToggleControl(Entity<FpvDroneLaptopComponent> ent, ref FpvDroneLaptopToggleControlBuiMsg args)
    {
        if (!TryComp<FpvDroneLaptopWatcherComponent>(args.Actor, out var watcher) ||
            watcher.Laptop != ent.Owner ||
            watcher.CurrentDrone is not { } droneNet ||
            !TryGetEntity(droneNet, out var droneUid) ||
            !TryComp<FpvDroneComponent>(droneUid.Value, out var drone))
        {
            _popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-select-drone"), ent, args.Actor);
            return;
        }

        if (watcher.ControlEnabled)
        {
            StopRemoteControl(args.Actor, watcher);
            return;
        }

        if (!_drone.IsControlLinkInRange(ent.Owner, droneUid.Value, drone))
        {
            _popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-no-signal"), ent, args.Actor);
            return;
        }

        if (!_drone.TryStartRemoteControl(droneUid.Value, ent.Owner, args.Actor, drone))
        {
            _popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-control-busy"), ent, args.Actor);
            return;
        }

        watcher.ControlEnabled = true;
        _mover.SetRelay(args.Actor, droneUid.Value);
        
        _popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-control-active"), args.Actor, args.Actor);

        Dirty(args.Actor, watcher);
        UpdateUI(ent);
    }

    private void OnDetonateDrone(Entity<FpvDroneLaptopComponent> ent, ref FpvDroneLaptopDetonateBuiMsg args)
    {
        if (!TryGetEntity(args.Drone, out var droneUid) || !ent.Comp.LinkedDrones.Contains(droneUid.Value))
            return;

        if (!TryComp<FpvDroneExplosiveComponent>(droneUid.Value, out var explosive))
            return;

        if (TryComp<FpvDroneLaptopWatcherComponent>(args.Actor, out var watcher) &&
            watcher.Laptop == ent.Owner &&
            watcher.CurrentDrone == args.Drone)
        {
            ClearWatcher(args.Actor, watcher);
        }

        _drone.TryTriggerExplosive(droneUid.Value, explosive);
        UpdateUI(ent);
    }

    private void OnWatcherShutdown(Entity<FpvDroneLaptopWatcherComponent> ent, ref ComponentShutdown args)
    {
        RemoveViewSubscription(ent.Owner, ent.Comp);

        if (ent.Comp.ControlEnabled)
            StopRemoteControl(ent.Owner, ent.Comp);
    }

    private void OnWatcherDetached(Entity<FpvDroneLaptopWatcherComponent> ent, ref PlayerDetachedEvent args)
    {
        RemoveViewSubscription(ent.Owner, ent.Comp, args.Player);

        if (ent.Comp.ControlEnabled)
            StopRemoteControl(ent.Owner, ent.Comp);

        ent.Comp.Laptop = null;
        ent.Comp.CurrentDrone = null;
        ent.Comp.ControlEnabled = false;
    }

    protected override void UnlinkDrone(Entity<FpvDroneLaptopComponent> laptop, EntityUid droneUid)
    {
        base.UnlinkDrone(laptop, droneUid);
        ClearWatchersForDroneImpl(droneUid);
        _drone.TryDisconnectDrone(droneUid);
    }

    protected override void ClearWatchersForDrone(EntityUid drone)
    {
        ClearWatchersForDroneImpl(drone);
    }

    protected override void UpdateUI(Entity<FpvDroneLaptopComponent> laptop)
    {
        if (!_ui.IsUiOpen(laptop.Owner, FpvDroneLaptopUiKey.Key))
            return;

        var state = new FpvDroneLaptopBuiState(BuildDroneInfoList(laptop));
        _ui.SetUiState(laptop.Owner, FpvDroneLaptopUiKey.Key, state);
    }

    private List<FpvDroneLaptopInfo> BuildDroneInfoList(Entity<FpvDroneLaptopComponent> laptop)
    {
        var list = new List<FpvDroneLaptopInfo>();
        var invalid = new List<EntityUid>();

        foreach (var droneUid in GetLinkedDrones(laptop))
        {
            if (!TryComp<FpvDroneComponent>(droneUid, out var drone))
            {
                invalid.Add(droneUid);
                continue;
            }

            var connected = _drone.IsControlLinkInRange(laptop.Owner, droneUid, drone);
            var health = GetDroneHealth(droneUid, out var maxHealth);
            var operatorName = drone.Pilot is { } pilot && Exists(pilot) ? Name(pilot) : null;

            list.Add(new FpvDroneLaptopInfo(
                GetNetEntity(droneUid),
                Name(droneUid),
                GetDroneRole(droneUid),
                health,
                maxHealth,
                connected,
                drone.SignalLost,
                drone.Pilot != null,
                operatorName,
                HasComp<FpvDroneExplosiveComponent>(droneUid)
            ));
        }

        foreach (var droneUid in invalid)
        {
            laptop.Comp.LinkedDrones.Remove(droneUid);
        }

        if (invalid.Count > 0)
            Dirty(laptop);

        return list;
    }

    private string GetDroneRole(EntityUid droneUid)
    {
        return HasComp<FpvDroneExplosiveComponent>(droneUid)
            ? Loc.GetString("cm-fpv-drone-role-explosive")
            : Loc.GetString("cm-fpv-drone-role-observer");
    }

    private float GetDroneHealth(EntityUid droneUid, out float maxHealth)
    {
        maxHealth = GetDroneMaxHealth(droneUid);
        var health = maxHealth;

        if (TryComp<DamageableComponent>(droneUid, out var damageable))
            health = Math.Max(0f, maxHealth - damageable.TotalDamage.Float());

        return health;
    }

    private float GetDroneMaxHealth(EntityUid droneUid)
    {
        if (!TryComp<DestructibleComponent>(droneUid, out var destructible))
            return 100f;

        var max = 0f;
        foreach (var threshold in destructible.Thresholds)
        {
            if (threshold.Trigger is DamageTrigger damage)
                max = Math.Max(max, damage.Damage);
        }

        return max > 0f ? max : 100f;
    }

    private void SetWatchedDrone(EntityUid user, FpvDroneLaptopWatcherComponent watcher, EntityUid laptop, EntityUid drone)
    {
        RemoveViewSubscription(user, watcher);

        if (!_actorQuery.TryComp(user, out var actor))
            return;

        watcher.Laptop = laptop;
        watcher.CurrentDrone = GetNetEntity(drone);
        watcher.ControlEnabled = false;
        Dirty(user, watcher);

        _viewSubscribers.AddViewSubscriber(drone, actor.PlayerSession);
    }

    private void RemoveViewSubscription(EntityUid user, FpvDroneLaptopWatcherComponent watcher, ICommonSession? session = null)
    {
        if (watcher.CurrentDrone is not { } current || !TryGetEntity(current, out var droneUid))
            return;

        if (session == null)
        {
            if (!_actorQuery.TryComp(user, out var actor))
                return;

            session = actor.PlayerSession;
        }

        _viewSubscribers.RemoveViewSubscriber(droneUid.Value, session);
    }

    private void StopRemoteControl(EntityUid user, FpvDroneLaptopWatcherComponent watcher)
    {
        if (watcher.CurrentDrone is { } current && TryGetEntity(current, out var droneUid))
        {
            _drone.StopRemoteControl(droneUid.Value, user);
        }

        if (TryComp<RelayInputMoverComponent>(user, out var relay) &&
            watcher.CurrentDrone is { } currentDrone &&
            TryGetEntity(currentDrone, out var relayDrone) &&
            relay.RelayEntity == relayDrone.Value)
        {
            RemComp(user, relay);
        }

        watcher.ControlEnabled = false;
        Dirty(user, watcher);
    }

    private void ClearWatcher(EntityUid user, FpvDroneLaptopWatcherComponent watcher)
    {
        RemoveViewSubscription(user, watcher);

        if (watcher.ControlEnabled)
            StopRemoteControl(user, watcher);

        watcher.Laptop = null;
        watcher.CurrentDrone = null;
        watcher.ControlEnabled = false;
        Dirty(user, watcher);
        RemCompDeferred<FpvDroneLaptopWatcherComponent>(user);
    }

    private void ClearWatchersForDroneImpl(EntityUid drone)
    {
        var droneNet = GetNetEntity(drone);
        var query = EntityQueryEnumerator<FpvDroneLaptopWatcherComponent>();
        while (query.MoveNext(out var uid, out var watcher))
        {
            if (watcher.CurrentDrone == droneNet)
                ClearWatcher(uid, watcher);
        }
    }

    private void CleanupWatchers()
    {
        var query = EntityQueryEnumerator<FpvDroneLaptopWatcherComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var watcher, out var xform))
        {
            if (watcher.Laptop is not { } laptopUid || !TryComp<FpvDroneLaptopComponent>(laptopUid, out _))
            {
                ClearWatcher(uid, watcher);
                continue;
            }

            var laptopXform = Transform(laptopUid);
            if (xform.MapID != laptopXform.MapID || 
                (_transform.GetWorldPosition(xform) - _transform.GetWorldPosition(laptopXform)).LengthSquared() > 4f)
            {
                ClearWatcher(uid, watcher);
                continue;
            }

            if (watcher.CurrentDrone is not { } droneNet || !TryGetEntity(droneNet, out var droneUid))
            {
                ClearWatcher(uid, watcher);
                continue;
            }

            if (!TryComp<FpvDroneComponent>(droneUid.Value, out var drone) ||
                !_drone.IsControlLinkInRange(laptopUid, droneUid.Value, drone))
            {
                ClearWatcher(uid, watcher);
            }
        }
    }

}
