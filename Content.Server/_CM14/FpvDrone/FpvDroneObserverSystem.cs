using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared.FpvDrone;
using Content.Shared.Raptor;
using Robust.Shared.Network;
using Robust.Shared.Maths;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.FpvDrone;

public sealed class FpvDroneObserverSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneObserverComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FpvDroneObserverComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<FpvDroneObserverComponent, RaptorEjectPilotEvent>(OnEject);
    }

    private void OnStartup(EntityUid uid, FpvDroneObserverComponent component, ComponentStartup args)
    {
        component.EjectAction = _action.AddAction(uid, component.EjectActionPrototypeId);

        if (TryComp<ActorComponent>(uid, out var actor))
        {
            RaiseNetworkEvent(new FpvDroneSetOverlayEvent(true), actor.PlayerSession);
        }
    }

    private void OnTerminating(EntityUid uid, FpvDroneObserverComponent component, EntityTerminatingEvent args)
    {
        Remove(uid, component);
    }

    private void OnEject(EntityUid uid, FpvDroneObserverComponent component, RaptorEjectPilotEvent args)
    {
        Remove(uid, component);
    }

    private void Remove(EntityUid uid, FpvDroneObserverComponent component)
    {
        if (!TryComp<FpvDroneControlComponent>(component.Control, out var control))
            return;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        if (control.Pilot == null)
            return;

        _mind.TransferTo(mindId, control.Pilot.Value, mind: mind);

        if (TryComp<ActorComponent>(control.Pilot.Value, out var actor))
        {
            RaiseNetworkEvent(new FpvDroneSetOverlayEvent(false), actor.PlayerSession);
        }

        control.Pilot = null;
        control.Observer = null;
    }
}