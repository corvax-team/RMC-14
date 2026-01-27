using System.Linq;
using Content.Shared.Body.Systems;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Examine;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Pathogen.Protomorphs.HiveCocoon;

public sealed class HiveCocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<ProtomorphComponent> _protomorphQuery;

    public override void Initialize()
    {
        _protomorphQuery = GetEntityQuery<ProtomorphComponent>();

        SubscribeLocalEvent<HiveCocoonComponent, ComponentStartup>(OnCocoonComponentStartup);
        SubscribeLocalEvent<HiveCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnCocoonGetAlternativeVerb);
        SubscribeLocalEvent<HiveCocoonComponent, CanDropTargetEvent>(OnCocoonCanDropTarget);
        SubscribeLocalEvent<HiveCocoonComponent, DragDropTargetEvent>(OnCocoonDragDropTarget);
        SubscribeLocalEvent<HiveCocoonComponent, DoAfterAttemptEvent<CocoonPupateDoAfterEvent>>(OnCocoonPupateDoAfterAttempt);
        SubscribeLocalEvent<HiveCocoonComponent, CocoonPupateDoAfterEvent>(OnCocoonPupateDoAfter);
        SubscribeLocalEvent<HiveCocoonComponent, ExaminedEvent>(OnCocoonExamined);
        SubscribeLocalEvent<HiveCocoonComponent, DestructionEventArgs>(OnCocoonDestruction);
    }

    private void OnCocoonComponentStartup(Entity<HiveCocoonComponent> cocoon, ref ComponentStartup args)
    {
        cocoon.Comp.MarineContainer = _container.EnsureContainer<Container>(cocoon, cocoon.Comp.MarineContainerId);
        cocoon.Comp.EquipmentContainer = _container.EnsureContainer<Container>(cocoon, cocoon.Comp.EquipmentContainerId);
        cocoon.Comp.BloodbursterSlot = _container.EnsureContainer<ContainerSlot>(cocoon, cocoon.Comp.BloodbursterSlotId);
    }

    private void OnCocoonGetAlternativeVerb(Entity<HiveCocoonComponent> cocoon, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (_pulling.GetPulling(user) is not { } target)
            return;

        if (!CanPupate(cocoon, user, target))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Priority = 100,
            Text = Loc.GetString("ccm-cocoon-verb-start-spinning"),
            Act = () => StartPupate(cocoon, user, target)
        });
    }

    private void OnCocoonCanDropTarget(Entity<HiveCocoonComponent> cocoon, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.CanDrop = CanPupate(cocoon, args.User, args.Dragged);
        args.Handled = true;
    }

    private void OnCocoonDragDropTarget(Entity<HiveCocoonComponent> cocoon, ref DragDropTargetEvent args)
    {
        args.Handled = StartPupate(cocoon, args.User, args.Dragged);
    }

    private void OnCocoonPupateDoAfterAttempt(Entity<HiveCocoonComponent> cocoon, ref DoAfterAttemptEvent<CocoonPupateDoAfterEvent> args)
    {
        if (args.DoAfter.Args.Target is { } target && !CanPupate(cocoon, args.DoAfter.Args.User, target))
            args.Cancel();
    }

    private void OnCocoonPupateDoAfter(Entity<HiveCocoonComponent> cocoon, ref CocoonPupateDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Target is not { } target)
            return;

        _container.Insert(target, cocoon.Comp.MarineContainer);
        if (cocoon.Comp.MarineContainer.ContainedEntities.Count >= cocoon.Comp.MaxContainedMarines)
        {
            TrySetState(cocoon, HiveCocoonState.Full);

            cocoon.Comp.PupateAt = _timing.CurTime + cocoon.Comp.PupateTime;
            DirtyField(cocoon, cocoon.Comp, nameof(cocoon.Comp.PupateAt));
        }
        else
        {
            TrySetState(cocoon, HiveCocoonState.Half);
        }
    }

    private void OnCocoonExamined(Entity<HiveCocoonComponent> cocoon, ref ExaminedEvent args)
    {
        if (!_protomorphQuery.HasComp(args.Examiner))
            return;

        using (args.PushGroup(nameof(HiveCocoonComponent)))
        {
            var cur = cocoon.Comp.MarineContainer.ContainedEntities.Count;
            var max = cocoon.Comp.MaxContainedMarines;
            args.PushMarkup(Loc.GetString("ccm-cocoon-examine-marines-count", ("cur-marines", cur), ("max-marines", max)));

            if (cur >= max)
                args.PushMarkup(Loc.GetString("ccm-cocoon-examine-remaining-time", ("time", (int)(cocoon.Comp.PupateAt - _timing.CurTime).TotalSeconds)));
        }
    }

    private void OnCocoonDestruction(Entity<HiveCocoonComponent> cocoon, ref DestructionEventArgs args)
    {
        var items = cocoon.Comp.EquipmentContainer;
        foreach (var item in items.ContainedEntities.ToArray())
        {
            _container.Remove(item, items);
        }

        var marines = cocoon.Comp.MarineContainer;
        foreach (var marine in marines.ContainedEntities.ToArray())
        {
            _container.Remove(marine, marines);
        }

        if (cocoon.Comp.BloodbursterSlot.ContainedEntity is { } bloodburster)
            _container.Remove(bloodburster, cocoon.Comp.BloodbursterSlot);
    }

    public void TrySetState(Entity<HiveCocoonComponent> cocoon, HiveCocoonState state)
    {
        if (state == cocoon.Comp.State)
            return;

        cocoon.Comp.State = state;
        _appearance.SetData(cocoon, HiveCocoonLayers.Base, state);
    }

    public bool TryStartPupate(Entity<HiveCocoonComponent?> cocoon, EntityUid user, EntityUid target)
    {
        if (!Resolve(cocoon, ref cocoon.Comp, false))
            return false;

        if (!CanPupate((cocoon, cocoon.Comp), user, target))
            return false;

        return StartPupate((cocoon, cocoon.Comp), user, target);
    }

    public bool StartPupate(Entity<HiveCocoonComponent> cocoon, EntityUid user, EntityUid target)
    {
        var ev = new CocoonPupateDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, user, cocoon.Comp.DoAfterTime, ev, cocoon, target, used: cocoon)
        {
            BreakOnMove = true,
            BreakOnRest = true,
            AttemptFrequency = AttemptFrequency.EveryTick
        };

        return _doAfter.TryStartDoAfter(doAfter);
    }

    public bool CanPupate(Entity<HiveCocoonComponent> cocoon, EntityUid user, EntityUid target)
    {
        if (!_protomorphQuery.HasComp(user))
            return false;

        if (cocoon.Comp.MarineContainer.ContainedEntities.Count >= cocoon.Comp.MaxContainedMarines)
            return false;

        if (cocoon.Comp.BloodbursterSlot.ContainedEntity != null)
            return false;

        if (!_standing.IsDown(target))
            return false;

        if (!_whitelist.IsWhitelistFailOrNull(cocoon.Comp.InsertWhitelist, user))
            return false;

        var attempt = new CocoonStartPupateAttemptEvent(cocoon, user, target);
        RaiseLocalEvent(cocoon, ref attempt);

        return !attempt.Cancelled;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HiveCocoonComponent>();
        while (query.MoveNext(out var uid, out var cocoon))
        {
            var time = _timing.CurTime;
            if (time >= cocoon.PupateAt && cocoon.State == HiveCocoonState.Full)
            {
                foreach (var marine in cocoon.MarineContainer.ContainedEntities.ToArray())
                {
                    _container.Insert(marine, cocoon.EquipmentContainer);
                    _body.GibBody(marine, false);
                }

                PredictedTrySpawnInContainer(cocoon.SpawnId, uid, cocoon.BloodbursterSlotId, out _);

                TrySetState((uid, cocoon), HiveCocoonState.Opening);
                cocoon.OpeningAt = _timing.CurTime + cocoon.OpeningTime;
                DirtyField(uid, cocoon, nameof(cocoon.OpeningAt));
            }

            if (time >= cocoon.OpeningAt && cocoon.State == HiveCocoonState.Opening)
            {
                if (cocoon.BloodbursterSlot.ContainedEntity is { } bloodburster)
                    _container.Remove(bloodburster, cocoon.BloodbursterSlot);

                TrySetState((uid, cocoon), HiveCocoonState.Empty);
            }
        }
    }
}
