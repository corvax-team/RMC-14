using Content.Server.Actions;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Mind;
using Content.Shared._CM14.FpvDrone;
using Content.Shared._RMC14.Explosion;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server._CM14.FpvDrone;

public sealed class FpvDroneSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneControlComponent, InteractHandEvent>(OnControlInteract);
        SubscribeLocalEvent<FpvDroneObserverComponent, ComponentStartup>(OnObserverStartup);

        SubscribeLocalEvent<FpvDroneObserverComponent, FpvDroneEjectEvent>((uid, comp, _) =>
            RemoveOverlayAndTransfer(uid, comp));
        SubscribeLocalEvent<FpvDroneObserverComponent, EntityTerminatingEvent>((uid, comp, _) =>
            RemoveOverlayAndTransfer(uid, comp));

        SubscribeLocalEvent<FpvDroneControlComponent, EntityTerminatingEvent>(OnControlTerminating);
        SubscribeLocalEvent<FpvDroneExplosiveComponent, ComponentInit>(OnExplosiveInit);
        SubscribeLocalEvent<FpvDroneExplosiveComponent, FpvDroneExplosiveEvent>(OnExplosiveAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FpvDroneObserverComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (TerminatingOrDeleted(uid) || !Exists(comp.Control))
                continue;

            if (!TryComp<FpvDroneControlComponent>(comp.Control, out var control))
                continue;

            if (control.Pilot is not { } pilot || !TryComp<FpvDroneScreenOverlayComponent>(pilot, out var screen))
                continue;

            var controlXform = Transform(comp.Control);
            var distSq = (xform.WorldPosition - controlXform.WorldPosition).LengthSquared();
            var maxRangeSq = comp.MaxRange * comp.MaxRange;

            var shouldHaveSignalLoss = distSq > maxRangeSq || xform.MapID != controlXform.MapID;

            if (screen.SignalLost != shouldHaveSignalLoss)
            {
                screen.SignalLost = shouldHaveSignalLoss;

                if (screen.SignalLost)
                    screen.TimeUntilExplosion = 0.8f;

                Dirty(pilot, screen);
            }

            if (!screen.SignalLost) continue;
            screen.TimeUntilExplosion -= frameTime;

            if (screen.TimeUntilExplosion <= 0) RaiseLocalEvent(uid, new FpvDroneExplosiveEvent());
        }
    }

    private void OnControlInteract(EntityUid uid, FpvDroneControlComponent component, InteractHandEvent args)
    {
        if (component.Used || !args.User.IsValid()) return;
        if (!_mind.TryGetMind(args.User, out var mindId, out var mind)) return;

        if (!_inventory.TryGetSlotEntity(args.User, "eyes", out var eyesUid) ||
            !HasComp<FpvDroneGogglesComponent>(eyesUid))
        {
            // Локализация для отсутствия очков
            _popup.PopupEntity(Loc.GetString("cm-fpv-drone-no-goggles"), args.User, args.User);
            return;
        }

        component.Used = true;
        var observer = Spawn(component.ObserverPrototypeId, _transform.GetMoverCoordinates(uid));
        var obsComp = EnsureComp<FpvDroneObserverComponent>(observer);
        obsComp.Control = uid;

        component.Observer = observer;
        component.Pilot = args.User;

        var screen = EnsureComp<FpvDroneScreenOverlayComponent>(args.User);
        screen.SignalLost = false;
        Dirty(args.User, screen);

        _mind.TransferTo(mindId, observer, mind: mind);
    }

    private void RemoveOverlayAndTransfer(EntityUid uid, FpvDroneObserverComponent component)
    {
        if (!TryComp<FpvDroneControlComponent>(component.Control, out var control)) 
            return;

        var pilot = control.Pilot;
        if (pilot == null || TerminatingOrDeleted(pilot.Value)) 
            return;

        if (TryComp<ActorComponent>(pilot.Value, out var actor))
            RaiseNetworkEvent(new FpvDroneSetOverlayEvent(false), actor.PlayerSession);

        RemComp<FpvDroneScreenOverlayComponent>(pilot.Value);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, pilot.Value, mind: mind);

        _popup.PopupEntity(Loc.GetString("fpv-drone-ui-connection-lost"), pilot.Value, pilot.Value, PopupType.LargeCaution);

        control.Pilot = null;
        control.Observer = null;
        control.Used = false;
    }

    private void OnObserverStartup(EntityUid uid, FpvDroneObserverComponent component, ComponentStartup args)
    {
        component.EjectAction = _action.AddAction(uid, component.EjectActionPrototypeId);
        if (TryComp<ActorComponent>(uid, out var actor))
            RaiseNetworkEvent(new FpvDroneSetOverlayEvent(true), actor.PlayerSession);
    }

    private void OnControlTerminating(EntityUid uid, FpvDroneControlComponent component, EntityTerminatingEvent args)
    {
        if (component.Observer is { } observer) QueueDel(observer);
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

        var mapCoords = _transform.GetMapCoordinates(uid);
        if (TryComp<FpvDroneObserverComponent>(uid, out var observer))
            RemoveOverlayAndTransfer(uid, observer);

        var triggeredEv = new CMExplosiveTriggeredEvent();
        RaiseLocalEvent(uid, ref triggeredEv);

        _explosion.QueueExplosion(mapCoords, comp.ExplosionType, comp.TotalIntensity, comp.MaxTileIntensity,
            comp.Radius, uid);
        QueueDel(uid);
    }
}