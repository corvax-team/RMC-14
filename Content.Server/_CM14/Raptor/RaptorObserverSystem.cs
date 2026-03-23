using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared.Raptor;

namespace Content.Server.Raptor;

public sealed class RaptorObserverSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RaptorObserverComponent, ComponentStartup>(OnSturtup);
        SubscribeLocalEvent<RaptorObserverComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<RaptorObserverComponent, RaptorEjectPilotEvent>(OnEject);
    }

    private void OnSturtup(EntityUid uid, RaptorObserverComponent component, ComponentStartup args)
    {
        component.EjectAction = _action.AddAction(uid, component.EjectActionPrototypeId);
    }

    private void OnRemove(EntityUid uid, RaptorObserverComponent component, ComponentRemove args)
    {
        Remove(uid, component);

        if (!TryComp<RaptorControlComponent>(component.Control, out var raprot))
            return;

        raprot.Observer = null;
    }

    private void OnEject(EntityUid uid, RaptorObserverComponent component, RaptorEjectPilotEvent args)
    {
        Remove(uid, component);
    }

    private void Remove(EntityUid uid, RaptorObserverComponent component)
    {
        if (!TryComp<RaptorControlComponent>(component.Control, out var raprot))
            return;

        //_action.RemoveAction(component.EjectAction);

        if (raprot != null)
            RemovePilot(uid, component);
    }

    public void RemovePilot(EntityUid uid, RaptorObserverComponent component)
    {
        if (!TryComp<RaptorControlComponent>(component.Control, out var control))
            return;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        if (control.Pilot == null)
            return;

        _mind.TransferTo(mindId, control.Pilot, mind: mind);

        control.Pilot = null;
    }
}