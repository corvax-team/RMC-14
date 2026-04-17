using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Dialog;
using Content.Shared._RMC14.Xenonids.Construction.EggMorpher;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.UserInterface;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Robust.Shared.GameObjects;

namespace Content.Server._RMC14.Xenonids.Parasite;

public sealed class XenoEggRoleSystem : EntitySystem
{
    private TimeSpan _parasiteSpawnDelay;

    [Dependency] private readonly ActorSystem _actor = default!;
    [Dependency] private readonly XenoEggSystem _eggSystem = default!;
    [Dependency] private readonly XenoParasiteThrowerSystem _throwerSystem = default!;
    [Dependency] private readonly EggMorpherSystem _eggMorpherSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly DialogSystem _dialog = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    public override void Initialize()
    {
        Subs.BuiEvents<EggMorpherComponent>(CCMXenoParasiteGhostUI.RegularKey, subs =>
        {
            subs.Event<CCMXenoParasiteGhostBuiMsg>(OnEggMorpherGhostBuiChosen);
        });

        SubscribeNetworkEvent<CCMGhostTakeParasiteEvent>(OnGhostTakeParasiteEvent);
        SubscribeNetworkEvent<CCMGhostTakeCarrierParasiteEvent>(OnGhostTakeCarrierParasiteEvent);
        SubscribeLocalEvent<DialogComponent, CCMTakeParasiteConfirmEvent>(OnDialogTakeParasiteConfirm);
        SubscribeLocalEvent<DialogComponent, CCMTakeCarrierParasiteConfirmEvent>(OnDialogTakeCarrierParasiteConfirm);
        SubscribeLocalEvent<XenoParasiteInfectEvent>((EntityEventHandler<XenoParasiteInfectEvent>)OnParasiteInfectSuccess);

        Subs.CVar(_config, RMCCVars.RMCParasiteSpawnInitialDelayMinutes, v => _parasiteSpawnDelay = TimeSpan.FromMinutes(v), true);
    }

    private void OnParasiteInfectSuccess(XenoParasiteInfectEvent ev)
    {
        if (!TryComp<MindContainerComponent>(ev.Parasite, out var mindContainer) || mindContainer.Mind == null)
            return;

        if (!TryComp<MindComponent>(mindContainer.Mind.Value, out var mind))
            return;

        var mindEntity = mind.CurrentEntity;
        if (mindEntity == null)
            return;

        if (HasComp<GhostComponent>(mindEntity.Value))
            EnsureComp<InfectionSuccessComponent>(ev.Parasite);
        else if (HasComp<XenoParasiteComponent>(mindEntity.Value) || HasComp<CCMRoyalParasiteComponent>(mindEntity.Value))
            EnsureComp<InfectionSuccessComponent>(ev.Parasite);
    }

    private void OnEggMorpherGhostBuiChosen(Entity<EggMorpherComponent> ent, ref CCMXenoParasiteGhostBuiMsg args)
    {
        var actor = GetEntity(args.Actor);

        if (!SharedChecks(ent, actor))
            return;

        var session = GetValidGhostSession(actor);
        if (session == null)
            return;

        if (ent.Comp.CurParasites > ent.Comp.ReservedParasites &&
            _eggMorpherSystem.TryCreateParasiteFromEggMorpher(ent, out var parasite) &&
            parasite.HasValue)
        {
            _ghostRole.GhostRoleInternalCreateMindAndTransfer(session, parasite.Value, parasite.Value);
        }
    }

    private void OnGhostTakeParasiteEvent(CCMGhostTakeParasiteEvent args, EntitySessionEventArgs sessionArgs)
    {
        var entity = GetEntity(args.ParasiteUid);

        if (!Exists(entity))
            return;

        var actor = sessionArgs.SenderSession.AttachedEntity;

        if (actor == null || !Exists(actor.Value))
            return;

        if (!UserCheck(actor.Value))
            return;

        if (!TryComp<GhostComponent>(actor.Value, out _))
            return;

        var title = args.IsRoyal
            ? Loc.GetString("rmc-xeno-parasite-take-royal-title")
            : Loc.GetString("rmc-xeno-parasite-take-title");
        var message = args.IsRoyal
            ? Loc.GetString("rmc-xeno-egg-ghost-royal-confirm")
            : Loc.GetString("rmc-xeno-egg-ghost-confirm");

        var confirmEvent = new CCMTakeParasiteConfirmEvent(GetNetEntity(entity), GetNetEntity(actor.Value), args.IsRoyal);
        _dialog.OpenConfirmation(actor.Value, title, message, confirmEvent);
    }

    private void OnGhostTakeCarrierParasiteEvent(CCMGhostTakeCarrierParasiteEvent args, EntitySessionEventArgs sessionArgs)
    {
        var carrier = GetEntity(args.CarrierUid);

        if (!Exists(carrier))
            return;

        var actor = sessionArgs.SenderSession.AttachedEntity;

        if (actor == null || !Exists(actor.Value))
            return;

        if (!UserCheck(actor.Value))
            return;

        if (!TryComp<GhostComponent>(actor.Value, out _))
            return;

        if (!TryComp<XenoParasiteThrowerComponent>(carrier, out var throwerComp))
            return;

        var isRoyal = args.IsRoyal;
        var totalParasites = isRoyal ? throwerComp.CurRoyalParasites : throwerComp.CurParasites;

        if (totalParasites <= 0)
        {
            TryShowPopup(actor.Value, Loc.GetString(isRoyal ? "rmc-xeno-throw-no-royal-parasites" : "rmc-xeno-throw-no-parasites"), PopupType.MediumCaution);
            return;
        }

        if (!_mobState.IsAlive(carrier))
        {
            TryShowPopup(actor.Value, Loc.GetString("rmc-xeno-egg-not-alive"), PopupType.MediumCaution);
            return;
        }

        var title = isRoyal
            ? Loc.GetString("rmc-xeno-parasite-take-royal-title")
            : Loc.GetString("rmc-xeno-parasite-take-title");
        var message = isRoyal
            ? Loc.GetString("rmc-xeno-egg-ghost-royal-confirm")
            : Loc.GetString("rmc-xeno-egg-ghost-confirm");

        var confirmEvent = new CCMTakeCarrierParasiteConfirmEvent(GetNetEntity(carrier), GetNetEntity(actor.Value), isRoyal);
        _dialog.OpenConfirmation(actor.Value, title, message, confirmEvent);
    }

    private bool TryShowPopup(EntityUid user, string message, PopupType popupType = PopupType.MediumCaution)
    {
        var curTime = _gameTiming.CurTime;
        var popupComp = EnsureComp<CCMLastPopupTimeComponent>(user);

        if (curTime < popupComp.LastPopupTime + TimeSpan.FromSeconds(5))
            return false;

        popupComp.LastPopupTime = curTime;
        _popup.PopupEntity(message, user, user, popupType);
        return true;
    }

    /// <param name="user"></param>
    /// <returns></returns>
    public bool UserCheck(EntityUid user)
    {
        if (_net.IsClient)
            return false;

        if (!TryComp(user, out GhostComponent? ghostComp))
            return false;

        var roundTime = _gameTicker.RoundDuration();
        if (roundTime <= _parasiteSpawnDelay)
        {
            TryShowPopup(user, Loc.GetString("rmc-xeno-egg-ghost-need-time-round", ("seconds", (int)(_parasiteSpawnDelay.TotalSeconds - roundTime.TotalSeconds))), PopupType.MediumCaution);
            return false;
        }

        if (HasComp<InfectionSuccessComponent>(user))
            return true;

        var timeSinceDeath = _gameTiming.CurTime.Subtract(ghostComp.TimeOfDeath);
        if (timeSinceDeath < TimeSpan.FromMinutes(3))
        {
            TryShowPopup(user, Loc.GetString("rmc-xeno-egg-ghost-need-time", ("seconds", 180 - (int)timeSinceDeath.TotalSeconds)), PopupType.MediumCaution);
            return false;
        }

        return true;
    }

    private bool SharedChecks(Entity<ParasiteAIComponent> parasite, EntityUid actor)
    {
        if (!TryComp<GhostComponent>(actor, out _))
            return false;

        if (!_mobState.IsAlive(parasite.Owner))
            return false;

        return true;
    }

    private ICommonSession? GetValidGhostSession(EntityUid actor)
    {
        if (!TryComp<ActorComponent>(actor, out var actorComp))
            return null;

        return actorComp.PlayerSession;
    }

    private bool SharedChecks<T>(Entity<T> spawner, EntityUid actor) where T : Component
    {
        if (!TryComp<GhostComponent>(actor, out _))
            return false;

        return true;
    }

    private void OnDialogTakeParasiteConfirm(Entity<DialogComponent> dialog, ref CCMTakeParasiteConfirmEvent ev)
    {
        ProcessTakeParasiteConfirm(ev);
    }

    private void ProcessTakeParasiteConfirm(CCMTakeParasiteConfirmEvent ev)
    {
        var entity = GetEntity(ev.ParasiteUid);
        var actor = GetEntity(ev.ActorUid);

        if (!Exists(entity) || !Exists(actor))
            return;

        var session = GetValidGhostSession(actor);
        if (session == null)
            return;

        if (TryComp<XenoEggComponent>(entity, out var egg))
        {
            if (_eggSystem.Open(new Entity<XenoEggComponent>(entity, egg), null, out var spawned) && spawned.HasValue)
            {
                Dirty(new Entity<XenoEggComponent>(entity, egg));
                _ghostRole.GhostRoleInternalCreateMindAndTransfer(session, spawned.Value, spawned.Value);
            }
            else
            {
                TryShowPopup(actor, Loc.GetString("rmc-xeno-egg-dead-child"), PopupType.MediumCaution);
            }
            return;
        }

        _ghostRole.GhostRoleInternalCreateMindAndTransfer(session, entity, entity);
    }

    private void OnDialogTakeCarrierParasiteConfirm(Entity<DialogComponent> dialog, ref CCMTakeCarrierParasiteConfirmEvent ev)
    {
        ProcessTakeCarrierParasiteConfirm(ev);
    }

    private void ProcessTakeCarrierParasiteConfirm(CCMTakeCarrierParasiteConfirmEvent ev)
    {
        var carrier = GetEntity(ev.CarrierUid);
        var actor = GetEntity(ev.ActorUid);

        if (!Exists(carrier) || !Exists(actor))
            return;

        var session = GetValidGhostSession(actor);
        if (session == null)
            return;

        if (!TryComp<XenoParasiteThrowerComponent>(carrier, out var throwerComp))
            return;

        EntityUid? parasite = null;
        if (ev.IsRoyal)
        {
            var container = _container.TryGetContainer(carrier, "royal_storage", out var royalStorage)
                ? royalStorage
                : null;
            if (container != null)
                parasite = container.ContainedEntities.FirstOrDefault(uid => HasComp<CCMRoyalParasiteComponent>(uid));
        }
        else
        {
            var container = _container.TryGetContainer(carrier, "parasite_storage", out var storage)
                ? storage
                : null;
            if (container != null)
                parasite = container.ContainedEntities.FirstOrDefault(uid => !HasComp<CCMRoyalParasiteComponent>(uid));
        }

        if (parasite == null || !Exists(parasite.Value))
        {
            TryShowPopup(actor, Loc.GetString("rmc-xeno-egg-dead-child"), PopupType.MediumCaution);
            return;
        }

        var parasiteContainer = _container.TryGetContainingContainer(parasite.Value, out var foundContainer) ? foundContainer : null;
        if (parasiteContainer != null)
        {
            _container.Remove(parasite.Value, parasiteContainer);
        }

        _throw.TryThrow(parasite.Value, new System.Numerics.Vector2(1, 0), 3);

        _ghostRole.GhostRoleInternalCreateMindAndTransfer(session, parasite.Value, parasite.Value);
    }
}
