using Content.Server.Mind;
using Content.Shared.Coordinates;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Server.Raptor;

public sealed class RaptorControlSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RaptorControlComponent, InteractHandEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, RaptorControlComponent component, InteractHandEvent args)
    {
        if (component.Pilot != null)
        {
            _popup.PopupClient("Already piloted!", uid, args.User);
            return;
        }

        AddPilot(uid, args.User, component);
    }

    private void AddPilot(EntityUid uid, EntityUid pilot, RaptorControlComponent component)
    {
        component.Pilot = pilot;

        if (!_mind.TryGetMind(pilot, out var mindId, out var mind))
            return;

        _mind.TransferTo(mindId, GetObserver(uid, component, Transform(pilot).Coordinates), mind: mind);
    }

    private EntityUid GetObserver(EntityUid uid, RaptorControlComponent component, EntityCoordinates? coordinates = null)
    {
        if (component.Observer == null)
        {
            var coords = coordinates;
            if (coords == null)
                coords = Transform(uid).Coordinates;

            var observer = Spawn(component.ObserverPrototypeId, (EntityCoordinates) coords);
            component.Observer = observer;

            EnsureComp<RaptorObserverComponent>(observer).Control = uid;
        }

        return (EntityUid)component.Observer;
    }
}