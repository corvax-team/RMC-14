using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._CM14.FpvDrone;
using Content.Shared._RMC14.Explosion;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._CM14.FpvDrone;

public sealed class FpvDroneSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRMCExplosionSystem _rmcExplosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneControlComponent, AfterInteractEvent>(OnControlAfterInteract);
        SubscribeLocalEvent<FpvDroneControlComponent, ActivateInWorldEvent>(OnControlActivate);
        SubscribeLocalEvent<FpvDroneObserverComponent, ComponentStartup>(OnObserverStartup);
        SubscribeLocalEvent<FpvDroneObserverComponent, FpvDroneEjectEvent>((uid, comp, _) =>
            RemoveOverlayAndTransfer(uid, comp));
        SubscribeLocalEvent<FpvDroneObserverComponent, EntityTerminatingEvent>(OnObserverTerminating);
        SubscribeLocalEvent<FpvDroneControlComponent, EntityTerminatingEvent>(OnControlTerminating);
        SubscribeLocalEvent<FpvDroneExplosiveComponent, ComponentInit>(OnExplosiveInit);
        SubscribeLocalEvent<FpvDroneExplosiveComponent, FpvDroneExplosiveEvent>(OnExplosiveAction);
        SubscribeLocalEvent<FpvDroneFoldableComponent, ActivateInWorldEvent>(OnFoldableActivate);
        SubscribeLocalEvent<FpvDroneFoldableComponent, FpvDroneFoldableDoAfterEvent>(OnFoldableDoAfter);
        SubscribeLocalEvent<FpvDroneControlComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerbs);
    }

    private void OnGetInteractionVerbs(EntityUid uid, FpvDroneControlComponent component,
        GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || component.Observer is not { Valid: true } droneUid)
            return;

        args.Verbs.Add(new InteractionVerb
        {
            Text = Loc.GetString("cm-fpv-drone-verb-unbind"),
            Act = () =>
            {
                if (TryComp<FpvDroneObserverComponent>(droneUid, out var observer))
                {
                    RemoveOverlayAndTransfer(droneUid, observer);
                    observer.Control = default;
                }

                _audio.PlayPvs(component.DisconnectedSound, uid);

                component.Observer = null;
                component.Used = false;

                _popup.PopupEntity(Loc.GetString("cm-fpv-drone-control-unlinked"), args.User, args.User);
            }
        });
    }

    private void OnControlAfterInteract(EntityUid uid, FpvDroneControlComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach)
            return;

        if (!TryComp<FpvDroneObserverComponent>(args.Target, out var observer))
            return;

        var target = args.Target.Value;
        component.Observer = target;
        observer.Control = uid;

        _audio.PlayPvs(component.ConnectedSound, uid);
        _popup.PopupEntity(Loc.GetString("cm-fpv-drone-control-linked"), target, args.User);
        args.Handled = true;
    }

    private void OnControlActivate(EntityUid uid, FpvDroneControlComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled) return;

        var user = args.User;

        if (component.Observer == null || TerminatingOrDeleted(component.Observer.Value))
        {
            _popup.PopupEntity(Loc.GetString("cm-fpv-drone-no-connection"), user, user);
            return;
        }

        if (!_inventory.TryGetSlotEntity(user, "eyes", out var eyesUid) ||
            !HasComp<FpvDroneGogglesComponent>(eyesUid))
        {
            _popup.PopupEntity(Loc.GetString("cm-fpv-drone-no-goggles"), user, user);
            return;
        }

        if (_mind.TryGetMind(user, out var mindId, out var mind))
        {
            var drone = component.Observer.Value;
            var obsComp = Comp<FpvDroneObserverComponent>(drone);

            obsComp.Pilot = user;
            component.Pilot = user;
            component.Used = true;

            if (obsComp is { FlyingLoopSound: not null, FlyingStream: null })
            {
                obsComp.FlyingStream = _audio.PlayPvs(obsComp.FlyingLoopSound, drone,
                    AudioParams.Default.WithLoop(true).WithVolume(-5f))?.Entity;
            }

            _mind.TransferTo(mindId, drone, mind: mind);
        }

        args.Handled = true;
    }

    private void RemoveOverlayAndTransfer(EntityUid uid, FpvDroneObserverComponent component, bool isSignalLost = false)
    {
        var pilot = component.Pilot;

        component.FlyingStream = _audio.Stop(component.FlyingStream);

        if (TryComp<FpvDroneControlComponent>(component.Control, out var control))
        {
            control.Pilot = null;
            control.Used = false;
        }

        if (pilot == null || TerminatingOrDeleted(pilot.Value))
        {
            component.Pilot = null;
            return;
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, pilot.Value, mind: mind);

        if (isSignalLost)
        {
            if (component.SignalLostSound != null)
            {
                _audio.PlayEntity(component.SignalLostSound, pilot.Value, pilot.Value,
                    AudioParams.Default.WithVolume(-2f));
            }

            _popup.PopupEntity(Loc.GetString("fpv-drone-ui-connection-lost"), pilot.Value, pilot.Value,
                PopupType.LargeCaution);
        }

        component.Pilot = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FpvDroneObserverComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var observer, out var droneXform))
        {
            if (observer.Pilot == null || TerminatingOrDeleted(uid) || !Exists(observer.Control))
                continue;

            var controlXform = Transform(observer.Control);
            var dronePos = _transform.GetWorldPosition(droneXform);
            var controlPos = _transform.GetWorldPosition(controlXform);
            var distSq = (dronePos - controlPos).LengthSquared();
            var maxRangeSq = observer.MaxRange * observer.MaxRange;

            if (distSq > maxRangeSq || droneXform.MapID != controlXform.MapID)
            {
                observer.SignalLost = true;
                RemoveOverlayAndTransfer(uid, observer, true);
            }
            else
                observer.SignalLost = false;
        }
    }

    private void OnFoldableActivate(EntityUid uid, FpvDroneFoldableComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled) return;
        if (_container.IsEntityInContainer(uid)) return;

        var ev = new FpvDroneFoldableDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.UnfoldDelay, ev, uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
            args.Handled = true;
    }

    private void OnFoldableDoAfter(EntityUid uid, FpvDroneFoldableComponent component,
        FpvDroneFoldableDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled) return;
        var coords = _transform.GetMoverCoordinates(uid);
        Spawn(component.UnfoldEntity, coords);
        QueueDel(uid);
        args.Handled = true;
    }

    private void OnObserverStartup(EntityUid uid, FpvDroneObserverComponent component, ComponentStartup args)
    {
        component.EjectAction = _action.AddAction(uid, component.EjectActionPrototypeId);
    }

    private void OnObserverTerminating(EntityUid uid, FpvDroneObserverComponent component, EntityTerminatingEvent args)
    {
        component.FlyingStream = _audio.Stop(component.FlyingStream);
        if (component.Pilot is { } pilot && !TerminatingOrDeleted(pilot))
        {
            if (_mind.TryGetMind(uid, out var mindId, out var mind))
                _mind.TransferTo(mindId, pilot, mind: mind);
        }

        component.Pilot = null;
    }

    private void OnControlTerminating(EntityUid uid, FpvDroneControlComponent component, EntityTerminatingEvent args)
    {
        if (component.Observer is { } observer)
            QueueDel(observer);
    }

    private void OnExplosiveInit(EntityUid uid, FpvDroneExplosiveComponent component, ComponentInit args)
    {
        if (component.ExplodeActionId != null)
            _action.AddAction(uid, ref component.ExplodeActionEntity, component.ExplodeActionId);
    }

    private void OnExplosiveAction(EntityUid uid, FpvDroneExplosiveComponent comp, FpvDroneExplosiveEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        if (TryComp<FpvDroneObserverComponent>(uid, out var observer))
            RemoveOverlayAndTransfer(uid, observer, true);

        var triggeredEv = new CMExplosiveTriggeredEvent();
        RaiseLocalEvent(uid, ref triggeredEv);
        _rmcExplosion.TriggerExplosive(uid, true, comp.TotalIntensity, comp.Radius);
    }
}