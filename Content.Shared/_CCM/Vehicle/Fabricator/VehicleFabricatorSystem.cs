using System.Collections.Immutable;
using System.Linq;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Vehicle.Fabricator;

public sealed class VehicleFabricatorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SkillsSystem _skills = default!;

    private int _startingPoints;
    private TimeSpan _gainEvery;

    public ImmutableArray<EntProtoId<VehicleFabricatorPrintableComponent>> Printables { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        SubscribeLocalEvent<VehicleFabricatorComponent, MapInitEvent>(OnFabricatorMapInit);
        SubscribeLocalEvent<VehicleFabricatorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<VehicleFabricatorComponent, VehicleFabricatoreRecycleDoafterEvent>(OnVehiclePartRecycled);

        Subs.BuiEvents<VehicleFabricatorComponent>(VehicleFabricatorUi.Key,
            subs =>
            {
                subs.Event<VehicleFabricatorPrintMsg>(OnPrintMsg);
            });

        Subs.CVar(_config, RMCCVars.VehicleFabricatorStartingPoints, v => _startingPoints = v, true);
        Subs.CVar(_config, RMCCVars.VehicleFabricatorGainEverySeconds, v => _gainEvery = TimeSpan.FromSeconds(v), true);

        ReloadPrototypes();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (ev.WasModified<EntityPrototype>())
            ReloadPrototypes();
    }

    private void OnFabricatorMapInit(Entity<VehicleFabricatorComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsServer)
            ent.Comp.Account = EnsurePoints();
    }

    private void OnInteractUsing(Entity<VehicleFabricatorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(args.Used, out VehicleFabricatorPrintableComponent? printable))
            return;

        args.Handled = true;

        var delay = printable.Delay;
        var multiplier = _skills.GetSkillDelayMultiplier(args.User, printable.RecycleSkill);
        delay *= multiplier;

        var ev = new VehicleFabricatoreRecycleDoafterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, delay, ev, ent, ent, args.Used)
        {
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnVehiclePartRecycled(Entity<VehicleFabricatorComponent> ent, ref VehicleFabricatoreRecycleDoafterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Used == null)
            return;

        if (!TryComp(args.Used, out VehicleFabricatorPrintableComponent? printable) ||
            !TryComp(ent.Comp.Account, out VehicleFabricatorPointsComponent? points))
        {
            return;
        }

        args.Handled = true;

        var refund = printable.Cost;
        points.Points += (int) (refund * printable.RecycleMultiplier);
        Dirty(ent.Comp.Account.Value, points);
        Del(args.Used.Value);

        _audio.PlayPvs(ent.Comp.RecycleSound, ent);
        _popup.PopupEntity(Loc.GetString("rmc-vehicle-fabricator-points", ("points", points.Points)), ent, args.User);
    }

    private void OnPrintMsg(Entity<VehicleFabricatorComponent> ent, ref VehicleFabricatorPrintMsg args)
    {
        if (args.Id == default || !_prototypes.TryIndex(args.Id, out var proto))
            return;

        if (!proto.TryGetComponent(out VehicleFabricatorPrintableComponent? printable, _compFactory))
            return;

        var actor = args.Actor;
        if (ent.Comp.Printing != null)
        {
            _popup.PopupClient(Loc.GetString("rmc-dropship-fabricator-busy"), actor, actor, PopupType.SmallCaution);
            return;
        }

        if (!TryComp(ent.Comp.Account, out VehicleFabricatorPointsComponent? points))
            return;

        if (printable.Cost > points.Points)
        {
            _popup.PopupClient(Loc.GetString("rmc-dropship-fabricator-not-enough-points"), actor, actor, PopupType.SmallCaution);
            return;
        }

        points.Points -= printable.Cost;
        Dirty(ent.Comp.Account.Value, points);

        ent.Comp.Points = points.Points;
        ent.Comp.Printing = proto.ID;
        ent.Comp.PrintAt = _timing.CurTime + printable.Delay;
        Dirty(ent);

        _appearance.SetData(ent, VehicleFabricatorVisuals.State, VehicleFabricatorState.Fabricating);
        _audio.PlayPvs(ent.Comp.ClickSound, ent);
    }

    private Entity<VehicleFabricatorPointsComponent> EnsurePoints()
    {
        var query = EntityQueryEnumerator<VehicleFabricatorPointsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            return (uid, comp);
        }

        var points = Spawn(null, MapCoordinates.Nullspace);
        var pointsComp = EnsureComp<VehicleFabricatorPointsComponent>(points);
        pointsComp.Points = _startingPoints;
        return (points, pointsComp);
    }

    private void ReloadPrototypes()
    {
        var printables = new List<EntityPrototype>();
        var prototypes = _prototypes.EnumeratePrototypes<EntityPrototype>();
        foreach (var prototype in prototypes)
        {
            if (prototype.HasComponent<VehicleFabricatorPrintableComponent>(_compFactory))
                printables.Add(prototype);
        }

        printables.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        Printables = printables.Select(e => new EntProtoId<VehicleFabricatorPrintableComponent>(e.ID)).ToImmutableArray();
    }

    private void SendUIStateAll(int points)
    {
        var fabricatorQuery = EntityQueryEnumerator<VehicleFabricatorComponent>();
        while (fabricatorQuery.MoveNext(out var fabricatorId, out var fabricator))
        {
            if (fabricator.Points == points)
                continue;

            fabricator.Points = points;
            Dirty(fabricatorId, fabricator);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;
        var allFabricatorQuery = EntityQueryEnumerator<VehicleFabricatorComponent>();
        while (allFabricatorQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.Printing == null || time < comp.PrintAt)
                continue;

            SpawnAtPosition(comp.Printing.Value, uid.ToCoordinates());

            _audio.PlayPvs(comp.PrintSound, uid);

            comp.Printing = null;
            Dirty(uid, comp);

            _appearance.SetData(uid, VehicleFabricatorVisuals.State, VehicleFabricatorState.Idle);
        }

        var pointsQuery = EntityQueryEnumerator<VehicleFabricatorPointsComponent>();
        while (pointsQuery.MoveNext(out var pointsId, out var points))
        {
            if (_gainEvery <= TimeSpan.Zero)
                continue;

            if (time < points.NextPointsAt)
                continue;

            points.NextPointsAt = time + _gainEvery;
            points.Points++;
            Dirty(pointsId, points);

            SendUIStateAll(points.Points);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class VehicleFabricatoreRecycleDoafterEvent : SimpleDoAfterEvent
{
}
