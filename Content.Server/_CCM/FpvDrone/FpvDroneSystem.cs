using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._CCM.FpvDrone;
using Content.Shared._RMC14.Explosion;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._CCM.FpvDrone;

public sealed class FpvDroneSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRMCExplosionSystem _rmcExplosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneComponent, ComponentStartup>(OnDroneStartup);
        SubscribeLocalEvent<FpvDroneComponent, FpvDroneEjectEvent>(OnDroneEject);
        SubscribeLocalEvent<FpvDroneComponent, EntityTerminatingEvent>(OnDroneTerminating);

        SubscribeLocalEvent<FpvDroneExplosiveComponent, ComponentInit>(OnExplosiveInit);
        SubscribeLocalEvent<FpvDroneExplosiveComponent, FpvDroneExplosiveEvent>(OnExplosiveAction);

        SubscribeLocalEvent<FpvDroneFoldableComponent, ActivateInWorldEvent>(OnFoldableActivate);
        SubscribeLocalEvent<FpvDroneFoldableComponent, FpvDroneFoldableDoAfterEvent>(OnFoldableDoAfter);
    }

    private void OnFoldableActivate(EntityUid uid, FpvDroneFoldableComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || _container.IsEntityInContainer(uid))
            return;

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
        if (args.Cancelled || args.Handled)
            return;

        var coords = _transform.GetMoverCoordinates(uid);
        Spawn(component.UnfoldEntity, coords);
        QueueDel(uid);
        args.Handled = true;
    }

    private void OnDroneStartup(EntityUid uid, FpvDroneComponent component, ComponentStartup args)
    {
        component.EjectAction = _action.AddAction(uid, component.EjectActionPrototypeId);
    }

    private void OnDroneEject(EntityUid uid, FpvDroneComponent component, FpvDroneEjectEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ReturnPilotToBody(uid, component);
    }

    private void OnDroneTerminating(EntityUid uid, FpvDroneComponent component, EntityTerminatingEvent args)
    {
        component.FlyingStream = _audio.Stop(component.FlyingStream);
        ReturnPilotToBody(uid, component, true);
    }

    private void OnExplosiveInit(EntityUid uid, FpvDroneExplosiveComponent component, ComponentInit args)
    {
        if (component.ExplodeActionId != null)
            _action.AddAction(uid, ref component.ExplodeActionEntity, component.ExplodeActionId);
    }

    private void OnExplosiveAction(EntityUid uid, FpvDroneExplosiveComponent component, FpvDroneExplosiveEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        TryTriggerExplosive(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FpvDroneComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var drone, out var droneXform))
        {
            if (drone.Pilot == null || TerminatingOrDeleted(uid))
                continue;

            if (!Exists(drone.Control))
            {
                ReturnPilotToBody(uid, drone, true);
                continue;
            }

            var controlXform = Transform(drone.Control);
            var dronePos = _transform.GetWorldPosition(droneXform);
            var controlPos = _transform.GetWorldPosition(controlXform);
            var distSq = (dronePos - controlPos).LengthSquared();
            var maxRangeSq = drone.MaxRange * drone.MaxRange;

            if (distSq > maxRangeSq || droneXform.MapID != controlXform.MapID)
                ReturnPilotToBody(uid, drone, true);
            else if (drone.SignalLost)
            {
                drone.SignalLost = false;
                Dirty(uid, drone);
            }
        }
    }

    private void ReturnPilotToBody(EntityUid uid, FpvDroneComponent component, bool isSignalLost = false)
    {
        var pilot = component.Pilot;
        component.FlyingStream = _audio.Stop(component.FlyingStream);

        if (isSignalLost && !component.SignalLost)
        {
            component.SignalLost = true;
            Dirty(uid, component);
        }

        if (pilot != null && !TerminatingOrDeleted(pilot.Value))
        {
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
        }

        component.Pilot = null;
        component.Control = default;
        component.SignalLost = false;
        Dirty(uid, component);
    }

    private void DisconnectDrone(EntityUid uid, FpvDroneComponent component)
    {
        ReturnPilotToBody(uid, component);
    }

    public bool IsControlLinkInRange(EntityUid control, EntityUid drone, FpvDroneComponent? component = null)
    {
        if (!Resolve(drone, ref component, false) || TerminatingOrDeleted(control) || TerminatingOrDeleted(drone))
            return false;

        var droneXform = Transform(drone);
        var controlXform = Transform(control);
        if (droneXform.MapID != controlXform.MapID)
            return false;

        var dronePos = _transform.GetWorldPosition(droneXform);
        var controlPos = _transform.GetWorldPosition(controlXform);
        var maxRangeSq = component.MaxRange * component.MaxRange;
        return (dronePos - controlPos).LengthSquared() <= maxRangeSq;
    }

    public bool TryStartRemoteControl(EntityUid drone, EntityUid control, EntityUid user, FpvDroneComponent? component = null)
    {
        if (!Resolve(drone, ref component, false) || TerminatingOrDeleted(user))
            return false;

        if (component.Pilot != null && component.Pilot != user)
            return false;

        if (component.Control != default && component.Control != control)
            return false;

        if (!IsControlLinkInRange(control, drone, component))
            return false;

        component.Control = control;
        component.Pilot = user;
        component.SignalLost = false;

        if (component is { FlyingLoopSound: not null, FlyingStream: null })
        {
            component.FlyingStream = _audio.PlayPvs(component.FlyingLoopSound, drone,
                AudioParams.Default.WithLoop(true).WithVolume(-5f))?.Entity;
        }

        Dirty(drone, component);
        return true;
    }

    public bool StopRemoteControl(EntityUid drone, EntityUid? expectedPilot = null, bool isSignalLost = false, FpvDroneComponent? component = null)
    {
        if (!Resolve(drone, ref component, false) || component.Pilot == null)
            return false;

        if (expectedPilot != null && component.Pilot != expectedPilot)
            return false;

        ReturnPilotToBody(drone, component, isSignalLost);
        return true;
    }

    public bool TryTriggerExplosive(EntityUid drone, FpvDroneExplosiveComponent? component = null)
    {
        if (!Resolve(drone, ref component, false))
            return false;

        if (TryComp<FpvDroneComponent>(drone, out var fpvDrone))
            ReturnPilotToBody(drone, fpvDrone, true);

        var triggeredEv = new CMExplosiveTriggeredEvent();
        RaiseLocalEvent(drone, ref triggeredEv);
        _rmcExplosion.TriggerExplosive(drone, true, component.TotalIntensity, component.Radius);
        return true;
    }

    public bool TryDisconnectDrone(EntityUid drone, FpvDroneComponent? component = null)
    {
        if (!Resolve(drone, ref component, false))
            return false;

        DisconnectDrone(drone, component);
        return true;
    }
}
