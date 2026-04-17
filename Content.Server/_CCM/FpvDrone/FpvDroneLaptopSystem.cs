using System.Linq;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Shared._CCM.FpvDrone;
using Content.Shared.Damage;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._CCM.FpvDrone;

public sealed class FpvDroneLaptopSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly FpvDroneSystem _drone = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscribers = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    private float _updateAccumulator;
    private const float UpdateInterval = 0.5f;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<FpvDroneLaptopComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<FpvDroneLaptopComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<FpvDroneLaptopComponent, ComponentShutdown>(OnLaptopShutdown);
        SubscribeLocalEvent<FpvDroneLaptopComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt);

        SubscribeLocalEvent<FpvDroneLaptopLinkedComponent, ComponentShutdown>(OnLinkedShutdown);
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
            subs.Event<FpvDroneLaptopUnlinkBuiMsg>(OnUnlinkDrone);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateAccumulator += frameTime;
        if (_updateAccumulator < UpdateInterval)
            return;

        _updateAccumulator = 0f;

        UpdateAllOpenUis();
        CleanupWatchers();
    }

    private void OnAfterInteract(Entity<FpvDroneLaptopComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!HasComp<PlaceableSurfaceComponent>(args.Target.Value))
            return;

        args.Handled = true;
        if (!_hands.TryDrop(args.User, ent.Owner, checkActionBlocker: false))
            return;

        PlaceLaptopOnSurface(ent, args.Target.Value);
    }

    private void OnNewLink(NewLinkEvent args)
    {
        if (!TryComp<FpvDroneLaptopComponent>(args.Source, out var laptop))
            return;

        if (!TryComp<FpvDroneComponent>(args.Sink, out var drone))
            return;

        if (args.SourcePort != "FpvDroneControl")
            return;

        var link = EnsureComp<FpvDroneLaptopLinkedComponent>(args.Sink);
        link.LinkedLaptop = args.Source;
        drone.Control = args.Source;
        laptop.LinkedDrones.Add(args.Sink);

        Dirty(args.Sink, link);
        Dirty(args.Sink, drone);
        Dirty(args.Source, laptop);

        if (args.User != null)
            _popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-linked"), args.Source, args.User.Value);

        UpdateUi((args.Source, laptop));
    }

    private void OnUiOpenAttempt(Entity<FpvDroneLaptopComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (IsOnSurface(ent.Owner))
            return;

        _popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-place-first"), ent, args.User);
        args.Cancel();
    }

    private void OnUiOpened(Entity<FpvDroneLaptopComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnUiClosed(Entity<FpvDroneLaptopComponent> ent, ref BoundUIClosedEvent args)
    {
        if (TryComp<FpvDroneLaptopWatcherComponent>(args.Actor, out var watcher) &&
            watcher.Laptop == ent.Owner)
        {
            ClearWatcher(args.Actor, watcher);
        }
    }

    private void OnParentChanged(Entity<FpvDroneLaptopComponent> ent, ref EntParentChangedMessage args)
    {
        var onSurface = IsOnSurface(ent.Owner);
        ent.Comp.IsOpen = onSurface;
        ent.Comp.IsPowered = onSurface;
        UpdateVisuals(ent);
        Dirty(ent);

        if (!onSurface)
        {
            _ui.CloseUi(ent.Owner, FpvDroneLaptopUiKey.Key);
            ClearAllWatchersForLaptop(ent.Owner);
        }
    }

    private void OnLaptopShutdown(Entity<FpvDroneLaptopComponent> ent, ref ComponentShutdown args)
    {
        UnlinkAll(ent);
        ClearAllWatchersForLaptop(ent.Owner);
    }

    private void OnLinkedShutdown(Entity<FpvDroneLaptopLinkedComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.LinkedLaptop is not { } laptopUid || !TryComp<FpvDroneLaptopComponent>(laptopUid, out var laptop))
            return;

        UnlinkDrone((laptopUid, laptop), ent.Owner);
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

        _mover.SetRelay(args.Actor, droneUid.Value);
        watcher.ControlEnabled = true;
        Dirty(args.Actor, watcher);
        UpdateUi(ent);
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
        UpdateUi(ent);
    }

    private void OnUnlinkDrone(Entity<FpvDroneLaptopComponent> ent, ref FpvDroneLaptopUnlinkBuiMsg args)
    {
        if (!TryGetEntity(args.Drone, out var droneUid))
            return;

        UnlinkDrone(ent, droneUid.Value);
        UpdateUi(ent);
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

    private void PlaceLaptopOnSurface(Entity<FpvDroneLaptopComponent> laptop, EntityUid surface)
    {
        var xform = Transform(surface);
        _transform.SetCoordinates(laptop.Owner, xform.Coordinates);
        _transform.SetParent(laptop.Owner, surface);

        laptop.Comp.IsOpen = true;
        laptop.Comp.IsPowered = true;
        UpdateVisuals(laptop);
        Dirty(laptop);
    }

    private void UnlinkDrone(Entity<FpvDroneLaptopComponent> laptop, EntityUid droneUid)
    {
        if (!laptop.Comp.LinkedDrones.Remove(droneUid))
            return;

        ClearWatchersForDrone(droneUid);
        _drone.TryDisconnectDrone(droneUid);

        if (TryComp<DeviceLinkSinkComponent>(droneUid, out var sink))
            _deviceLink.RemoveAllFromSink(droneUid, sink);

        if (TryComp<FpvDroneLaptopLinkedComponent>(droneUid, out var linked))
            RemComp(droneUid, linked);

        Dirty(laptop);
    }

    private void UnlinkAll(Entity<FpvDroneLaptopComponent> laptop)
    {
        foreach (var drone in laptop.Comp.LinkedDrones)
        {
            UnlinkDrone(laptop, drone);
        }
    }

    private List<EntityUid> GetLinkedDrones(Entity<FpvDroneLaptopComponent> laptop)
    {
        var linked = new List<EntityUid>();

        if (TryComp<DeviceLinkSourceComponent>(laptop, out var source))
        {
            foreach (var sink in source.LinkedPorts.Keys)
            {
                if (HasComp<FpvDroneComponent>(sink))
                    linked.Add(sink);
            }

            laptop.Comp.LinkedDrones = linked.ToHashSet();
            return linked;
        }

        linked.AddRange(laptop.Comp.LinkedDrones);
        return linked;
    }

    private void UpdateAllOpenUis()
    {
        var query = EntityQueryEnumerator<FpvDroneLaptopComponent>();
        while (query.MoveNext(out var uid, out var laptop))
        {
            if (_ui.IsUiOpen(uid, FpvDroneLaptopUiKey.Key))
                UpdateUi((uid, laptop));
        }
    }

    private void UpdateUi(Entity<FpvDroneLaptopComponent> laptop)
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
            _drone.StopRemoteControl(droneUid.Value);

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

    private void ClearAllWatchersForLaptop(EntityUid laptop)
    {
        var query = EntityQueryEnumerator<FpvDroneLaptopWatcherComponent>();
        while (query.MoveNext(out var uid, out var watcher))
        {
            if (watcher.Laptop == laptop)
                ClearWatcher(uid, watcher);
        }
    }

    private void ClearWatchersForDrone(EntityUid drone)
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
        var query = EntityQueryEnumerator<FpvDroneLaptopWatcherComponent>();
        while (query.MoveNext(out var uid, out var watcher))
        {
            if (watcher.Laptop is not { } laptopUid || !TryComp<FpvDroneLaptopComponent>(laptopUid, out _))
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

    private bool IsOnSurface(EntityUid laptop)
    {
        var parent = Transform(laptop).ParentUid;
        return HasComp<PlaceableSurfaceComponent>(parent);
    }

    private void UpdateVisuals(Entity<FpvDroneLaptopComponent> laptop)
    {
        var state = FpvDroneLaptopState.Closed;
        if (laptop.Comp.IsOpen)
            state = laptop.Comp.IsPowered ? FpvDroneLaptopState.Active : FpvDroneLaptopState.Open;

        _appearance.SetData(laptop, FpvDroneLaptopVisuals.State, state);
    }
}
