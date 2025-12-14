using System.Linq;
using System.Numerics;
using Content.Server.Hands.Systems;
using Content.Server.Players;
using Content.Server.Mind;
using Content.Server.Ghost.Roles;
using Content.Server.GameTicking;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Damage.ObstacleSlamming;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Hands.Components;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Ghost;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server._RMC14.Xenonids.Parasite;

public sealed partial class XenoParasiteThrowerSystem : SharedXenoParasiteThrowerSystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly SharedXenoParasiteSystem _parasite = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RMCObstacleSlammingSystem _rmcObstacleSlamming = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ActorSystem _actor = default!;

    private void InitializeParasiteThrower()
    {
        SubscribeLocalEvent<XenoParasiteThrowerComponent, XenoThrowParasiteActionEvent>(OnThrowParasite);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, CCMXenoThrowRoyalParasiteActionEvent>(OnThrowRoyalParasite);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, UserActivateInWorldEvent>(OnXenoParasiteThrowerUseInHand);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, XenoEvolutionDoAfterEvent>(OnXenoEvolveDoAfter);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, XenoDevolveBuiMsg>(OnXenoDevolveDoAfter);
    }

    public override void Initialize()
    {
        base.Initialize();
        InitializeParasiteThrower();
    }

    private void OnThrowParasite(Entity<XenoParasiteThrowerComponent> xeno, ref XenoThrowParasiteActionEvent args)
    {
        args.Handled = true;

        if (TryComp<HandsComponent>(xeno.Owner, out var handsComp) &&
            _hands.TryGetActiveItem(new Entity<HandsComponent?>(xeno.Owner, handsComp), out var heldEntity))
        {
            if (HasComp<CCMRoyalParasiteComponent>(heldEntity.Value))
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-wrong-parasite-type-royal"), xeno, xeno);
                return;
            }
        }

        OnToggleParasiteThrow(xeno, ref args, false);
    }

    private void OnThrowRoyalParasite(Entity<XenoParasiteThrowerComponent> xeno, ref CCMXenoThrowRoyalParasiteActionEvent args)
    {
        args.Handled = true;

        if (TryComp<HandsComponent>(xeno.Owner, out var handsComp) &&
            _hands.TryGetActiveItem(new Entity<HandsComponent?>(xeno.Owner, handsComp), out var heldEntity))
        {
            if (HasComp<XenoParasiteComponent>(heldEntity.Value) && !HasComp<CCMRoyalParasiteComponent>(heldEntity.Value))
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-wrong-parasite-type-regular"), xeno, xeno);
                return;
            }
        }

        var parasiteArgs = new XenoThrowParasiteActionEvent { Target = args.Target, Action = args.Action };
        OnToggleParasiteThrow(xeno, ref parasiteArgs, true);
    }

    private void OnToggleParasiteThrow(Entity<XenoParasiteThrowerComponent> xeno, ref XenoThrowParasiteActionEvent args, bool isRoyal = false)
    {
        var target = args.Target;

        args.Handled = true;

        _action.SetUseDelay((args.Action, args.Action), TimeSpan.Zero);

        if (_interact.InRangeUnobstructed(xeno, target))
        {
            var pickupRange = isRoyal ? xeno.Comp.RoyalParasitePickupRange : xeno.Comp.ParasitePickupRange;

            var entitiesInRange = _lookup.GetEntitiesInRange(target, pickupRange);

            foreach (var possibleParasite in entitiesInRange)
            {
                if (_mobState.IsDead(possibleParasite))
                    continue;

                if (!HasComp<XenoParasiteComponent>(possibleParasite))
                    continue;

                if (!HasComp<ParasiteAIComponent>(possibleParasite))
                    continue;

                if (Transform(possibleParasite).ParentUid.IsValid() &&
                    TryComp<ContainerManagerComponent>(Transform(possibleParasite).ParentUid, out _))
                    continue;

                var parasiteIsRoyal = HasComp<CCMRoyalParasiteComponent>(possibleParasite);

                if (parasiteIsRoyal != isRoyal)
                    continue;
                if (parasiteIsRoyal)
                {
                    if (xeno.Comp.CurRoyalParasites >= xeno.Comp.MaxRoyalParasites)
                    {
                        if (!_hands.TryGetEmptyHand(xeno.Owner, out _))
                        {
                            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-royals", ("current", xeno.Comp.CurRoyalParasites), ("max", xeno.Comp.MaxRoyalParasites)), xeno, xeno);
                            return;
                        }
                        _hands.TryPickupAnyHand(xeno, possibleParasite);
                        xeno.Comp.CurRoyalParasitesInHands++;
                        Dirty(xeno);
                        return;
                    }
                }
                else
                {
                    if (xeno.Comp.CurParasites >= xeno.Comp.MaxParasites)
                    {
                        if (!_hands.TryGetEmptyHand(xeno.Owner, out _))
                        {
                            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-parasites", ("current", xeno.Comp.CurParasites), ("max", xeno.Comp.MaxParasites)), xeno, xeno);
                            return;
                        }
                        _hands.TryPickupAnyHand(xeno, possibleParasite);
                        xeno.Comp.CurParasitesInHands++;
                        Dirty(xeno);
                        return;
                    }
                }

                AddParasite(possibleParasite, xeno, isRoyal);
                if (isRoyal)
                    _popup.PopupEntity(Loc.GetString("cm-xeno-throw-parasite-stash-royal", ("cur_royals", xeno.Comp.CurRoyalParasites), ("max_royals", xeno.Comp.MaxRoyalParasites)), xeno, xeno);
                else
                    _popup.PopupEntity(Loc.GetString("cm-xeno-throw-parasite-stash-parasite", ("cur_parasites", xeno.Comp.CurParasites), ("max_parasites", xeno.Comp.MaxParasites)), xeno, xeno);
                return;
            }
        }

        if (TryComp<HandsComponent>(xeno.Owner, out var handsComp) &&
            _hands.GetActiveItem(new Entity<HandsComponent?>(xeno.Owner, handsComp)) is { } heldEntity &&
            HasComp<XenoParasiteComponent>(heldEntity))
        {
            var parasiteIsRoyal = HasComp<CCMRoyalParasiteComponent>(heldEntity);

            _hands.TryDrop(xeno.Owner);

            if (parasiteIsRoyal)
                xeno.Comp.CurRoyalParasitesInHands = Math.Max(0, xeno.Comp.CurRoyalParasitesInHands - 1);
            else
                xeno.Comp.CurParasitesInHands = Math.Max(0, xeno.Comp.CurParasitesInHands - 1);

            var coords = _transform.GetMoverCoordinates(xeno);
            var maxThrowDistance = parasiteIsRoyal ? xeno.Comp.RoyalParasiteThrowDistance : xeno.Comp.ParasiteThrowDistance;

            if (coords.TryDistance(EntityManager, target, out var dis) && dis > maxThrowDistance)
            {
                var fixedTrajectory = (target.Position - coords.Position).Normalized() * maxThrowDistance;
                target = coords.WithPosition(coords.Position + fixedTrajectory);
            }

            _rmcObstacleSlamming.MakeImmune(heldEntity);
            _throw.TryThrow(heldEntity, target, user: xeno);

            if (TryComp<ParasiteAIComponent>(heldEntity, out var ai) && !_mobState.IsDead(heldEntity))
            {
                var stunDuration = parasiteIsRoyal ? xeno.Comp.ThrownRoyalParasiteStunDuration : xeno.Comp.ThrownParasiteStunDuration;
                _stun.TryStun(heldEntity, stunDuration * (parasiteIsRoyal ? 1.5f : 2f), true);
                _parasite.GoActive((heldEntity, ai));
            }

            var cooldown = parasiteIsRoyal ? xeno.Comp.ThrownRoyalParasiteCooldown : xeno.Comp.ThrownParasiteCooldown;
            _action.SetUseDelay((args.Action, args.Action), cooldown);

            Dirty(xeno);
            return;
        }

        if (isRoyal)
        {
            var totalRoyalParasites = xeno.Comp.CurRoyalParasites + xeno.Comp.CurRoyalParasitesInHands;
            if (totalRoyalParasites <= 0)
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-royal-parasites"), xeno, xeno);
                return;
            }
        }
        else
        {
            var totalRegularParasites = xeno.Comp.CurParasites + xeno.Comp.CurParasitesInHands;
            if (totalRegularParasites <= 0)
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-parasites"), xeno, xeno);
                return;
            }
        }

        if (!_hands.TryGetEmptyHand(xeno.Owner, out _))
            return;

        if (HasComp<OnFireComponent>(xeno))
        {
            _popup.PopupEntity("Retrieving a stored parasite while we're on fire would burn it!", xeno, args.Performer, PopupType.MediumCaution);
            return;
        }

        if (RemoveParasite(xeno, isRoyal) is not { } newParasite)
            return;

        _hive.SetSameHive(xeno.Owner, newParasite);

        _hands.TryPickupAnyHand(xeno, newParasite);

        string msg;
        if (isRoyal)
            msg = Loc.GetString("cm-xeno-throw-parasite-unstash-royal", ("cur_royals", xeno.Comp.CurRoyalParasites), ("max_royals", xeno.Comp.MaxRoyalParasites));
        else
            msg = Loc.GetString("cm-xeno-throw-parasite-unstash-parasite", ("cur_parasites", xeno.Comp.CurParasites), ("max_parasites", xeno.Comp.MaxParasites));
        _popup.PopupEntity(msg, xeno, xeno);
    }

    private void OnXenoParasiteThrowerUseInHand(Entity<XenoParasiteThrowerComponent> xeno, ref UserActivateInWorldEvent args)
    {
        var target = args.Target;

        if (!HasComp<XenoParasiteComponent>(target))
            return;

        if (_mobState.IsDead(target))
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-egg-dead-child"), xeno, xeno);
            return;
        }

        if (args.Handled)
            return;

        var isRoyal = HasComp<CCMRoyalParasiteComponent>(target);

        bool storageFull = false;
        if (isRoyal)
        {
            storageFull = xeno.Comp.CurRoyalParasites >= xeno.Comp.MaxRoyalParasites;
        }
        else
        {
            storageFull = xeno.Comp.CurParasites >= xeno.Comp.MaxParasites;
        }

        if (storageFull)
        {
            if (!_hands.TryGetEmptyHand(xeno.Owner, out _))
            {
                if (isRoyal)
                    _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-royals", ("current", xeno.Comp.CurRoyalParasites), ("max", xeno.Comp.MaxRoyalParasites)), xeno, xeno);
                else
                    _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-parasites", ("current", xeno.Comp.CurParasites), ("max", xeno.Comp.MaxParasites)), xeno, xeno);
                return;
            }

            if (_hands.TryPickupAnyHand(xeno, target))
            {
                if (isRoyal)
                    xeno.Comp.CurRoyalParasitesInHands++;
                else
                    xeno.Comp.CurParasitesInHands++;
                Dirty(xeno);
                args.Handled = true;
                return;
            }
            else
            {
                if (isRoyal)
                    _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-royals", ("current", xeno.Comp.CurRoyalParasites), ("max", xeno.Comp.MaxRoyalParasites)), xeno, xeno);
                else
                    _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-parasite-too-many-parasites", ("current", xeno.Comp.CurParasites), ("max", xeno.Comp.MaxParasites)), xeno, xeno);
                return;
            }
        }

        if (_mind.TryGetMind(target, out _, out _))
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-egg-awake-child", ("parasite", target)), xeno, xeno);
            return;
        }

        AddParasite(target, xeno, isRoyal);

        string msg;
        if (isRoyal)
            msg = Loc.GetString("cm-xeno-throw-parasite-stash-royal", ("cur_royals", xeno.Comp.CurRoyalParasites), ("max_royals", xeno.Comp.MaxRoyalParasites));
        else
            msg = Loc.GetString("cm-xeno-throw-parasite-stash-parasite", ("cur_parasites", xeno.Comp.CurParasites), ("max_parasites", xeno.Comp.MaxParasites));

        _popup.PopupEntity(msg, xeno, xeno);

        args.Handled = true;
    }

    private void OnXenoEvolveDoAfter(Entity<XenoParasiteThrowerComponent> xeno, ref XenoEvolutionDoAfterEvent args)
    {
        DropAllStoredParasites(xeno);
    }

    private void OnXenoDevolveDoAfter(Entity<XenoParasiteThrowerComponent> xeno, ref XenoDevolveBuiMsg args)
    {
        DropAllStoredParasites(xeno);
    }

    private void OnMobStateChanged(Entity<XenoParasiteThrowerComponent> xeno, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        DropAllStoredParasites(xeno, 0.75f);
    }

    private bool DropAllStoredParasites(Entity<XenoParasiteThrowerComponent> xeno, float chance = 1.0f)
    {
        TryComp(xeno, out XenoComponent? _);

        if (chance != 1.0 && (xeno.Comp.CurParasites > 0 || xeno.Comp.CurRoyalParasites > 0 || xeno.Comp.CurParasitesInHands > 0 || xeno.Comp.CurRoyalParasitesInHands > 0))
            _popup.PopupEntity(Loc.GetString("rmc-xeno-parasite-carrier-death", ("xeno", xeno)), xeno, PopupType.MediumCaution);

        var hive = _hive.GetHive(xeno.Owner);

        for (var i = 0; i < xeno.Comp.CurParasites; ++i)
        {
            if (chance != 1.0 && !_random.Prob(chance))
                continue;
            var newParasite = Spawn(xeno.Comp.ParasitePrototype);
            _hive.SetHive(newParasite, hive);
            _transform.DropNextTo(newParasite, xeno.Owner);
            _stun.TryStun(newParasite, xeno.Comp.ThrownParasiteStunDuration, true);
            _throw.TryThrow(newParasite, _random.NextAngle().RotateVec(Vector2.One) * _random.NextFloat(0.15f, 0.7f), 3);
        }

        for (var i = 0; i < xeno.Comp.CurRoyalParasites; ++i)
        {
            if (chance != 1.0 && !_random.Prob(chance))
                continue;
            var newParasite = Spawn(xeno.Comp.RoyalParasitePrototype);
            _hive.SetHive(newParasite, hive);
            _transform.DropNextTo(newParasite, xeno.Owner);
            _stun.TryStun(newParasite, xeno.Comp.ThrownParasiteStunDuration, true);
            _throw.TryThrow(newParasite, _random.NextAngle().RotateVec(Vector2.One) * _random.NextFloat(0.15f, 0.7f), 3);
        }

        for (var i = 0; i < xeno.Comp.CurParasitesInHands; ++i)
        {
            if (chance != 1.0 && !_random.Prob(chance))
                continue;
            var newParasite = Spawn(xeno.Comp.ParasitePrototype);
            _hive.SetHive(newParasite, hive);
            _transform.DropNextTo(newParasite, xeno.Owner);
            _stun.TryStun(newParasite, xeno.Comp.ThrownParasiteStunDuration, true);
            _throw.TryThrow(newParasite, _random.NextAngle().RotateVec(Vector2.One) * _random.NextFloat(0.15f, 0.7f), 3);
        }

        for (var i = 0; i < xeno.Comp.CurRoyalParasitesInHands; ++i)
        {
            if (chance != 1.0 && !_random.Prob(chance))
                continue;
            var newParasite = Spawn(xeno.Comp.RoyalParasitePrototype);
            _hive.SetHive(newParasite, hive);
            _transform.DropNextTo(newParasite, xeno.Owner);
            _stun.TryStun(newParasite, xeno.Comp.ThrownParasiteStunDuration, true);
            _throw.TryThrow(newParasite, _random.NextAngle().RotateVec(Vector2.One) * _random.NextFloat(0.15f, 0.7f), 3);
        }

        xeno.Comp.CurParasites = 0;
        xeno.Comp.CurRoyalParasites = 0;
        xeno.Comp.CurParasitesInHands = 0;
        xeno.Comp.CurRoyalParasitesInHands = 0;

        UpdateParasiteClingers(xeno);
        return true;
    }

    private void AddParasite(EntityUid parasite, Entity<XenoParasiteThrowerComponent> xeno, bool isRoyal = false)
    {
        if (isRoyal)
            xeno.Comp.CurRoyalParasites++;
        else
            xeno.Comp.CurParasites++;

        UpdateParasiteClingers(xeno);

        QueueDel(parasite);
    }

    private EntityUid? RemoveParasite(Entity<XenoParasiteThrowerComponent> xeno, bool isRoyal = false)
    {
        if (isRoyal)
        {
            if (xeno.Comp.CurRoyalParasites <= 0)
                return null;
            xeno.Comp.CurRoyalParasites--;
        }
        else
        {
            if (xeno.Comp.CurParasites <= 0)
                return null;
            xeno.Comp.CurParasites--;
        }

        var prototype = isRoyal ? xeno.Comp.RoyalParasitePrototype : xeno.Comp.ParasitePrototype;
        var parasite = Spawn(prototype);

        UpdateParasiteClingers(xeno);
        Dirty(xeno);
        return parasite;
    }

    private void UpdateParasiteClingers(Entity<XenoParasiteThrowerComponent> xeno)
    {
        var parasiteNumber = Math.Min(Math.Ceiling((((double)xeno.Comp.CurParasites / xeno.Comp.MaxParasites) * xeno.Comp.NumPositions)), xeno.Comp.NumPositions);

        var overlayNumbers = xeno.Comp.VisiblePositions.Count(position => position == true);

        if (overlayNumbers > parasiteNumber)
        {
            var visibleIndexes = GetVisualIndexes(xeno.Comp.VisiblePositions, true);
            for (var i = 0; i < overlayNumbers - parasiteNumber; i++)
            {
                var index = _random.PickAndTake(visibleIndexes);
                xeno.Comp.VisiblePositions[index] = false;
            }
        }
        else
        {
            var invisibleIndexes = GetVisualIndexes(xeno.Comp.VisiblePositions, false);
            for (var i = 0; i < parasiteNumber - overlayNumbers; i++)
            {
                var index = _random.PickAndTake(invisibleIndexes);
                xeno.Comp.VisiblePositions[index] = true;
            }
        }

        Dirty(xeno);

        //Need to clone the array for it to dirty properly
        _appearance.SetData(xeno, ParasiteOverlayVisuals.States, xeno.Comp.VisiblePositions.Clone());
    }

    private List<int> GetVisualIndexes(bool[] bools, bool visible)
    {
        List<int> visualIndexes = new();
        for (int i = 0; i < bools.Length; i++)
        {
            if (bools[i] == visible)
                visualIndexes.Add(i);
        }
        return visualIndexes;
    }

    public bool HasRoyalParasites(Entity<XenoParasiteThrowerComponent> xeno)
    {
        return xeno.Comp.CurRoyalParasites > xeno.Comp.ReservedRoyalParasites;
    }

    public EntityUid? TryRemoveGhostParasite(Entity<XenoParasiteThrowerComponent> xeno, out string message)
    {
        message = "";
        var regularParasites = xeno.Comp.CurParasites - xeno.Comp.CurRoyalParasites;
        var regularInHands = xeno.Comp.CurParasitesInHands - xeno.Comp.CurRoyalParasitesInHands;
        var totalRegularParasites = regularParasites + regularInHands;

        if (totalRegularParasites <= 0)
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-none", ("xeno", xeno));
            return null;
        }

        if (xeno.Comp.ReservedParasites >= totalRegularParasites)
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-reserved", ("xeno", xeno));
            return null;
        }

        if (_mobState.IsDead(xeno))
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-dead", ("xeno", xeno));
            return null;
        }

        var spawnedParasite = RemoveParasite(xeno);
        if (spawnedParasite == null)
            return null;

        var parasite = spawnedParasite.Value;
        _hive.SetSameHive(xeno.Owner, parasite);
        _rmcObstacleSlamming.MakeImmune(parasite);
        _transform.DropNextTo(parasite, xeno.Owner);
        _throw.TryThrow(parasite, _random.NextAngle().RotateVec(Vector2.One), 3);

        return parasite;
    }

    public EntityUid? TryRemoveGhostRoyalParasite(Entity<XenoParasiteThrowerComponent> xeno, out string message)
    {
        message = "";
        var totalRoyalParasites = xeno.Comp.CurRoyalParasites + xeno.Comp.CurRoyalParasitesInHands;
        if (totalRoyalParasites <= 0)
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-royal-none", ("xeno", xeno));
            return null;
        }

        if (xeno.Comp.ReservedRoyalParasites >= totalRoyalParasites)
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-reserved", ("xeno", xeno));
            return null;
        }

        if (_mobState.IsDead(xeno))
        {
            message = Loc.GetString("rmc-xeno-parasite-ghost-carrier-dead", ("xeno", xeno));
            return null;
        }

        var container = _container.EnsureContainer<Container>(xeno.Owner, "royal_storage");
        var para = container.ContainedEntities.FirstOrDefault(uid => HasComp<CCMRoyalParasiteComponent>(uid));

        EntityUid parasite;
        if (para == default)
        {
            var newParasite = RemoveParasite(xeno, true);
            if (newParasite == null)
                return null;
            parasite = newParasite.Value;
        }
        else
        {
            parasite = para;
            _container.Remove(para, container);
            xeno.Comp.CurRoyalParasites--;
            Dirty(xeno);
        }

        _hive.SetSameHive(xeno.Owner, parasite);
        _rmcObstacleSlamming.MakeImmune(parasite);
        _transform.DropNextTo(parasite, xeno.Owner);
        _throw.TryThrow(parasite, _random.NextAngle().RotateVec(Vector2.One), 3);

        return parasite;
    }

    public void HandleGhostParasitePossession(Entity<XenoParasiteThrowerComponent> xeno, EntityUid ghost, bool isRoyal)
    {
        if (!TryComp(ghost, out GhostComponent? ghostComp))
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-parasite-ghost-invalid"), xeno, ghost, PopupType.MediumCaution);
            return;
        }

        var roundTime = _gameTicker.RoundDuration();
        var parasiteSpawnDelay = TimeSpan.FromMinutes(_config.GetCVar(RMCCVars.RMCParasiteSpawnInitialDelayMinutes));

        if (roundTime <= parasiteSpawnDelay)
        {
            var secondsNeeded = (int)(parasiteSpawnDelay.TotalSeconds - roundTime.TotalSeconds);
            _popup.PopupEntity(Loc.GetString("rmc-xeno-egg-ghost-need-time-round", ("seconds", secondsNeeded)), ghost, ghost, PopupType.MediumCaution);
            return;
        }

        if (!HasComp<InfectionSuccessComponent>(ghost))
        {
            var timeSinceDeath = _gameTiming.CurTime.Subtract(ghostComp.TimeOfDeath);
            if (timeSinceDeath < TimeSpan.FromMinutes(3))
            {
                var secondsNeeded = 180 - (int)timeSinceDeath.TotalSeconds;
                _popup.PopupEntity(Loc.GetString("rmc-xeno-egg-ghost-need-time", ("seconds", secondsNeeded)), ghost, ghost, PopupType.MediumCaution);
                return;
            }
        }

        EntityUid? parasiteUid = null;
        string errorMessage = "";

        if (isRoyal)
        {
            parasiteUid = TryRemoveGhostRoyalParasite(xeno, out errorMessage);
        }
        else
        {
            parasiteUid = TryRemoveGhostParasite(xeno, out errorMessage);
        }

        if (parasiteUid == null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                _popup.PopupEntity(errorMessage, xeno, ghost, PopupType.MediumCaution);
            else
                _popup.PopupEntity(Loc.GetString("rmc-xeno-parasite-ghost-take-failed"), xeno, ghost, PopupType.MediumCaution);
            return;
        }

        if (!_actor.TryGetSession(ghost, out var session) || session == null)
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-parasite-ghost-no-session"), xeno, ghost, PopupType.MediumCaution);
            return;
        }

        _ghostRole.GhostRoleInternalCreateMindAndTransfer(session, parasiteUid.Value, parasiteUid.Value);
    }
}

