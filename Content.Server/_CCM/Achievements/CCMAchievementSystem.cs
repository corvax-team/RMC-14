// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.KillTracking;
using Content.Server._CCM.Stats;
using Content.Shared._CCM.Achievements;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.Construction;
using Content.Shared._RMC14.Entrenching;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Construction.Events;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Server._RMC14.Xenonids.Construction.ResinHole;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Projectiles;
using Content.Shared.Vehicle.Components;
using Content.Shared._RMC14.Vehicle;
using Robust.Shared.Log;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Achievements;

public sealed class CCMAchievementSystem : EntitySystem
{
    private const string LogCategory = "ccm.achievements";
    private const float LiveProgressFlushIntervalSeconds = 10f;

    private static readonly HashSet<string> OfficerJobs =
    [
        "CMCommandingOfficer",
        "CMExecutiveOfficer",
        "CMStaffOfficer",
        "CMAuxiliarySupportOfficer",
        "CMIntelOfficer",
        "CMLogisticsOfficer",
    ];

    private static readonly List<CCMAchievementDefinition> Definitions =
    [
        new("general_first_steps", CCMAchievementCategory.General, "ccm-achievement-general-first-steps-title", "ccm-achievement-general-first-steps-desc", 10, ctx => ctx.RoundsPlayed),
        new("general_veteran", CCMAchievementCategory.General, "ccm-achievement-general-veteran-title", "ccm-achievement-general-veteran-desc", 50, ctx => ctx.RoundsPlayed),
        new("general_living_legend", CCMAchievementCategory.General, "ccm-achievement-general-living-legend-title", "ccm-achievement-general-living-legend-desc", 200, ctx => ctx.RoundsPlayed),
        new("general_first_victory", CCMAchievementCategory.General, "ccm-achievement-general-first-victory-title", "ccm-achievement-general-first-victory-desc", 10, ctx => ctx.RoundsWon),
        new("general_campaign_veteran", CCMAchievementCategory.General, "ccm-achievement-general-campaign-veteran-title", "ccm-achievement-general-campaign-veteran-desc", 50, ctx => ctx.RoundsWon),
        new("general_war_legend", CCMAchievementCategory.General, "ccm-achievement-general-war-legend-title", "ccm-achievement-general-war-legend-desc", 200, ctx => ctx.RoundsWon),
        new("general_beta_tester", CCMAchievementCategory.General, "ccm-achievement-general-beta-tester-title", "ccm-achievement-general-beta-tester-desc", 1, _ => 0, true),
        new("general_founding_member", CCMAchievementCategory.General, "ccm-achievement-general-founding-member-title", "ccm-achievement-general-founding-member-desc", 1, _ => 0, true),

        new("misc_logistician", CCMAchievementCategory.Misc, "ccm-achievement-misc-logistician-title", "ccm-achievement-misc-logistician-desc", 20, ctx => ctx.Special.RequisitionOrders),
        new("misc_queen_slayer", CCMAchievementCategory.Misc, "ccm-achievement-misc-queen-slayer-title", "ccm-achievement-misc-queen-slayer-desc", 1, ctx => ctx.Special.QueenKillParticipations),
        new("misc_friendly_fire", CCMAchievementCategory.Misc, "ccm-achievement-misc-friendly-fire-title", "ccm-achievement-misc-friendly-fire-desc", 300, ctx => ctx.Special.FriendlyFireDamage),
        new("misc_quality_assurance", CCMAchievementCategory.Misc, "ccm-achievement-misc-quality-assurance-title", "ccm-achievement-misc-quality-assurance-desc", 1, _ => 0, true),

        new("marine_field_medic", CCMAchievementCategory.Marines, "ccm-achievement-marine-field-medic-title", "ccm-achievement-marine-field-medic-desc", 5000, ctx => ctx.MarineHealingDone),
        new("marine_combat_surgeon", CCMAchievementCategory.Marines, "ccm-achievement-marine-combat-surgeon-title", "ccm-achievement-marine-combat-surgeon-desc", 25000, ctx => ctx.MarineHealingDone),
        new("marine_guardian_angel", CCMAchievementCategory.Marines, "ccm-achievement-marine-guardian-angel-title", "ccm-achievement-marine-guardian-angel-desc", 100000, ctx => ctx.MarineHealingDone),
        new("marine_legendary_medic", CCMAchievementCategory.Marines, "ccm-achievement-marine-legendary-medic-title", "ccm-achievement-marine-legendary-medic-desc", 250000, ctx => ctx.MarineHealingDone),

        new("marine_corpsman", CCMAchievementCategory.Marines, "ccm-achievement-marine-corpsman-title", "ccm-achievement-marine-corpsman-desc", 25, ctx => ctx.MarineRevives),
        new("marine_paramedic", CCMAchievementCategory.Marines, "ccm-achievement-marine-paramedic-title", "ccm-achievement-marine-paramedic-desc", 100, ctx => ctx.MarineRevives),
        new("marine_savior", CCMAchievementCategory.Marines, "ccm-achievement-marine-savior-title", "ccm-achievement-marine-savior-desc", 300, ctx => ctx.MarineRevives),

        new("marine_mechanic", CCMAchievementCategory.Marines, "ccm-achievement-marine-mechanic-title", "ccm-achievement-marine-mechanic-desc", 50, ctx => ctx.MarineStructuresBuilt),
        new("marine_fortifier", CCMAchievementCategory.Marines, "ccm-achievement-marine-fortifier-title", "ccm-achievement-marine-fortifier-desc", 500, ctx => ctx.MarineStructuresBuilt),
        new("marine_defense_architect", CCMAchievementCategory.Marines, "ccm-achievement-marine-defense-architect-title", "ccm-achievement-marine-defense-architect-desc", 2000, ctx => ctx.MarineStructuresBuilt),

        new("marine_victory", CCMAchievementCategory.Marines, "ccm-achievement-marine-victory-title", "ccm-achievement-marine-victory-desc", 10, ctx => ctx.MarineRoundsWon),
        new("marine_campaigns_veteran", CCMAchievementCategory.Marines, "ccm-achievement-marine-campaigns-veteran-title", "ccm-achievement-marine-campaigns-veteran-desc", 50, ctx => ctx.MarineRoundsWon),
        new("marine_corps_legend", CCMAchievementCategory.Marines, "ccm-achievement-marine-corps-legend-title", "ccm-achievement-marine-corps-legend-desc", 200, ctx => ctx.MarineRoundsWon),
        new("marine_commander", CCMAchievementCategory.Marines, "ccm-achievement-marine-commander-title", "ccm-achievement-marine-commander-desc", 1, ctx => ctx.Special.OfficerWins),

        new("marine_recruit", CCMAchievementCategory.Marines, "ccm-achievement-marine-recruit-title", "ccm-achievement-marine-recruit-desc", 100, ctx => ctx.MarineKills),
        new("marine_bug_hunter", CCMAchievementCategory.Marines, "ccm-achievement-marine-bug-hunter-title", "ccm-achievement-marine-bug-hunter-desc", 500, ctx => ctx.MarineKills),
        new("marine_exterminator", CCMAchievementCategory.Marines, "ccm-achievement-marine-exterminator-title", "ccm-achievement-marine-exterminator-desc", 1000, ctx => ctx.MarineKills),
        new("marine_hive_nightmare", CCMAchievementCategory.Marines, "ccm-achievement-marine-hive-nightmare-title", "ccm-achievement-marine-hive-nightmare-desc", 2500, ctx => ctx.MarineKills),
        new("marine_hive_genocide", CCMAchievementCategory.Marines, "ccm-achievement-marine-hive-genocide-title", "ccm-achievement-marine-hive-genocide-desc", 5000, ctx => ctx.MarineKills),

        new("xeno_hive_growth", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-hive-growth-title", "ccm-achievement-xeno-hive-growth-desc", 10, ctx => ctx.XenoRoundsWon),
        new("xeno_domination", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-domination-title", "ccm-achievement-xeno-domination-desc", 50, ctx => ctx.XenoRoundsWon),
        new("xeno_hive_empire", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-hive-empire-title", "ccm-achievement-xeno-hive-empire-desc", 150, ctx => ctx.XenoRoundsWon),

        new("xeno_hive_birth", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-hive-birth-title", "ccm-achievement-xeno-hive-birth-desc", 1, ctx => ctx.Special.XenoEvolutions),
        new("xeno_resin_worker", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-resin-worker-title", "ccm-achievement-xeno-resin-worker-desc", 50, ctx => ctx.XenoStructuresBuilt),
        new("xeno_hive_fortifier", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-hive-fortifier-title", "ccm-achievement-xeno-hive-fortifier-desc", 500, ctx => ctx.XenoStructuresBuilt),
        new("xeno_hive_architect", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-hive-architect-title", "ccm-achievement-xeno-hive-architect-desc", 2000, ctx => ctx.XenoStructuresBuilt),
        new("xeno_young_hunter", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-young-hunter-title", "ccm-achievement-xeno-young-hunter-desc", 50, ctx => ctx.XenoKills),
        new("xeno_predator", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-predator-title", "ccm-achievement-xeno-predator-desc", 250, ctx => ctx.XenoKills),
        new("xeno_drop_horror", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-drop-horror-title", "ccm-achievement-xeno-drop-horror-desc", 500, ctx => ctx.XenoKills),
        new("xeno_marine_nightmare", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-marine-nightmare-title", "ccm-achievement-xeno-marine-nightmare-desc", 1000, ctx => ctx.XenoKills),
        new("xeno_apex_predator", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-apex-predator-title", "ccm-achievement-xeno-apex-predator-desc", 3000, ctx => ctx.XenoKills),
        new("xeno_queen_wrath", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-queen-wrath-title", "ccm-achievement-xeno-queen-wrath-desc", 10, ctx => ctx.Special.QueenKills),
        new("xeno_planet_mistress", CCMAchievementCategory.Xenos, "ccm-achievement-xeno-planet-mistress-title", "ccm-achievement-xeno-planet-mistress-desc", 1, ctx => ctx.Special.QueenWins),
    ];

    [Dependency] private readonly CCMStatsSystem _stats = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private readonly Dictionary<NetUserId, CachedAchievementState> _cache = new();
    private readonly Dictionary<NetUserId, RoundAchievementState> _round = new();
    private bool _roundFinalized;
    private bool _flushingLiveProgress;
    private float _liveProgressFlushAccumulator;
    private CCMStatsSide _winningSide = CCMStatsSide.None;

    public IReadOnlyList<string> GetAchievementIds()
    {
        return Definitions.Select(def => def.Id).ToArray();
    }

    public bool TryNormalizeAchievementId(string input, out string achievementId)
    {
        achievementId = string.Empty;
        var match = Definitions.FirstOrDefault(def => def.Id.Equals(input, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(match.Id))
            return false;

        achievementId = match.Id;
        return true;
    }

    public async Task<bool> GrantAchievementAsync(NetUserId userId, string achievementId, bool notify = true)
    {
        try
        {
            if (!TryNormalizeAchievementId(achievementId, out var normalizedId))
                return false;

            Logger.InfoS(LogCategory, $"GrantAchievementAsync started for user {userId} achievement '{normalizedId}' (notify={notify}).");

            var state = await EnsureStateLoadedAsync(userId);
            if (state == null || !state.UnlockedIds.Add(normalizedId))
                return false;

            await PersistUnlockedIdsAsync(userId, state);

            if (notify && _players.TryGetSessionById(userId, out var session))
            {
                var context = BuildContext(userId, state);
                var def = Definitions.First(definition => definition.Id == normalizedId);
                var completedCount = CountCompleted(context, state.UnlockedIds);

                RaiseNetworkEvent(
                    new CCMAchievementUnlockedEvent(def.ToProgress(def.Goal, true), completedCount, Definitions.Count),
                    session.Channel);
            }

            await PushSnapshotAsync(userId, state);
            Logger.InfoS(LogCategory, $"GrantAchievementAsync finished for user {userId} achievement '{normalizedId}'.");
            return true;
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"GrantAchievementAsync failed for user {userId} achievement '{achievementId}': {e}");
            return false;
        }
    }

    public async Task<bool> RevokeAchievementAsync(NetUserId userId, string achievementId)
    {
        try
        {
            if (!TryNormalizeAchievementId(achievementId, out var normalizedId))
                return false;

            Logger.InfoS(LogCategory, $"RevokeAchievementAsync started for user {userId} achievement '{normalizedId}'.");

            var state = await EnsureStateLoadedAsync(userId);
            if (state == null || !state.UnlockedIds.Remove(normalizedId))
                return false;

            await PersistUnlockedIdsAsync(userId, state);
            await PushSnapshotAsync(userId, state);
            Logger.InfoS(LogCategory, $"RevokeAchievementAsync finished for user {userId} achievement '{normalizedId}'.");
            return true;
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"RevokeAchievementAsync failed for user {userId} achievement '{achievementId}': {e}");
            return false;
        }
    }

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestCCMAchievementsEvent>(OnRequestAchievements);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CCMStatsSystem.CCMCombatDamageRecordedEvent>(OnCombatDamageRecorded);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
        SubscribeLocalEvent<TargetDefibrillatedEvent>(OnTargetDefibrillated);
        SubscribeLocalEvent<RMCStructureBuiltEvent>(OnMarineStructureBuilt, after: [typeof(CCMStatsSystem)]);
        SubscribeLocalEvent<XenoStructureBuiltEvent>(OnXenoStructureBuilt, after: [typeof(CCMStatsSystem)]);
        SubscribeLocalEvent<XenoStructureUpgradedEvent>(OnXenoStructureUpgraded, after: [typeof(CCMStatsSystem)]);
        SubscribeLocalEvent<XenoResinHolePlacedEvent>(OnXenoResinHolePlaced, after: [typeof(CCMStatsSystem)]);
        SubscribeLocalEvent<XenoTunnelPlacedEvent>(OnXenoTunnelPlaced, after: [typeof(CCMStatsSystem)]);
        SubscribeLocalEvent<NewXenoEvolvedEvent>(OnNewXenoEvolved);
        SubscribeLocalEvent<CCMRequisitionOrderedEvent>(OnRequisitionOrdered);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend,
            after: [typeof(CCMStatsSystem)]);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_roundFinalized || _flushingLiveProgress || _round.Count == 0)
            return;

        _liveProgressFlushAccumulator += frameTime;
        if (_liveProgressFlushAccumulator < LiveProgressFlushIntervalSeconds)
            return;

        _liveProgressFlushAccumulator = 0f;
        _ = FlushLiveProgressAsync();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _round.Clear();
        _cache.Clear();
        _roundFinalized = false;
        _flushingLiveProgress = false;
        _liveProgressFlushAccumulator = 0f;
        _winningSide = CCMStatsSide.None;
    }

    private async void OnRequestAchievements(RequestCCMAchievementsEvent ev, EntitySessionEventArgs args)
    {
        try
        {
            var state = await EnsureStateLoadedAsync(args.SenderSession.UserId);
            if (state == null)
                return;

            var snapshot = BuildSnapshot(args.SenderSession.UserId, state);
            RaiseNetworkEvent(new CCMAchievementsResponseEvent(snapshot), args.SenderSession.Channel);
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"OnRequestAchievements failed for user {args.SenderSession.UserId}: {e}");
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        var round = GetOrCreateRoundState(ev.Player.UserId);
        round.MarineParticipated |= HasComp<MarineComponent>(ev.Mob);
        round.XenoParticipated |= HasComp<XenoComponent>(ev.Mob);
        round.OfficerParticipated |= ev.JobId != null && OfficerJobs.Contains(ev.JobId);
        round.QueenParticipated |= HasComp<XenoEvolutionGranterComponent>(ev.Mob);

        _ = EnsureStateLoadedAsync(ev.Player.UserId);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var round = GetOrCreateRoundState(ev.Player.UserId);
        round.MarineParticipated |= HasComp<MarineComponent>(ev.Entity);
        round.XenoParticipated |= HasComp<XenoComponent>(ev.Entity);
        round.QueenParticipated |= HasComp<XenoEvolutionGranterComponent>(ev.Entity);

        _ = EnsureStateLoadedAsync(ev.Player.UserId);
    }

    private void OnCombatDamageRecorded(CCMStatsSystem.CCMCombatDamageRecordedEvent ev)
    {
        if (!ev.FriendlyFire || ev.Damage <= 0)
            return;

        GetOrCreateRoundState(ev.UserId).FriendlyFireDamage += (int) MathF.Round(ev.Damage);
        _ = EvaluatePlayerAsync(ev.UserId, notify: true);
    }

    private void OnKillReported(ref KillReportedEvent args)
    {
        if (args.Suicide)
            return;

        var participants = GetKillParticipantPlayers(args);

        foreach (var participant in participants)
        {
            var round = GetOrCreateRoundState(participant);

            if (HasComp<XenoEvolutionGranterComponent>(args.Entity) && HasComp<XenoComponent>(args.Entity))
                round.QueenKillParticipations += 1;
        }

        if (args.Primary is KillPlayerSource player &&
            _players.TryGetSessionById(player.PlayerId, out var session) &&
            session.AttachedEntity is { } attached &&
            HasComp<XenoEvolutionGranterComponent>(attached) &&
            HasComp<XenoComponent>(attached) &&
            HasComp<MarineComponent>(args.Entity))
        {
            GetOrCreateRoundState(player.PlayerId).QueenKills += 1;
        }

        foreach (var participant in participants)
        {
            _ = EvaluatePlayerAsync(participant, notify: true, pushSnapshot: true);
        }
    }

    private void OnTargetDefibrillated(ref TargetDefibrillatedEvent ev)
    {
        if (!TryComp(ev.User, out ActorComponent? actor))
            return;

        _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnMarineStructureBuilt(RMCStructureBuiltEvent args)
    {
        if (TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnXenoStructureBuilt(XenoStructureBuiltEvent args)
    {
        if (TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnXenoStructureUpgraded(XenoStructureUpgradedEvent args)
    {
        if (TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnXenoResinHolePlaced(XenoResinHolePlacedEvent args)
    {
        if (TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnXenoTunnelPlaced(XenoTunnelPlacedEvent args)
    {
        if (TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnNewXenoEvolved(ref NewXenoEvolvedEvent args)
    {
        if (!TryComp(args.NewXeno, out ActorComponent? actor))
            return;

        var round = GetOrCreateRoundState(actor.PlayerSession.UserId);
        round.XenoEvolutions += 1;
        round.XenoParticipated = true;
        round.QueenParticipated |= HasComp<XenoEvolutionGranterComponent>(args.NewXeno);

        _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true, pushSnapshot: true);
    }

    private void OnRequisitionOrdered(CCMRequisitionOrderedEvent ev)
    {
        GetOrCreateRoundState(ev.UserId).RequisitionOrders += 1;
        _ = EvaluatePlayerAsync(ev.UserId, notify: true, pushSnapshot: true);
    }

    private async void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (_roundFinalized || !TryGetWinningSide(out _winningSide))
            return;

        _roundFinalized = true;
        var roundEntries = _round.ToArray();
        var evaluationUsers = _round.Keys
            .Concat(_stats.GetTrackedPlayers())
            .Distinct()
            .ToArray();

        try
        {
            foreach (var (userId, round) in roundEntries)
            {
                if (_winningSide == CCMStatsSide.Marines && round.OfficerParticipated)
                    round.OfficerWins += 1;

                if (_winningSide == CCMStatsSide.Xenos && round.QueenParticipated)
                    round.QueenWins += 1;

                if (!round.HasAnyProgress)
                    continue;

                var friendlyFireDamage = Math.Max(0, round.FriendlyFireDamage - round.PersistedFriendlyFireDamage);
                var requisitionOrders = Math.Max(0, round.RequisitionOrders - round.PersistedRequisitionOrders);
                var xenoEvolutions = Math.Max(0, round.XenoEvolutions - round.PersistedXenoEvolutions);
                var officerWins = Math.Max(0, round.OfficerWins - round.PersistedOfficerWins);
                var queenKills = Math.Max(0, round.QueenKills - round.PersistedQueenKills);
                var queenWins = Math.Max(0, round.QueenWins - round.PersistedQueenWins);
                var queenKillParticipations = Math.Max(0, round.QueenKillParticipations - round.PersistedQueenKillParticipations);

                await _db.AdjustCCMPlayerAchievementStats(
                    userId.UserId,
                    friendlyFireDamage,
                    requisitionOrders,
                    xenoEvolutions,
                    officerWins,
                    queenKills,
                    queenWins,
                    queenKillParticipations);
            }

            foreach (var userId in evaluationUsers)
            {
                _ = EvaluatePlayerAsync(userId, notify: true, pushSnapshot: true);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to finalize CCM achievements:\n{e}");
        }
    }

    private async Task FlushLiveProgressAsync()
    {
        if (_flushingLiveProgress || _roundFinalized)
            return;

        _flushingLiveProgress = true;

        try
        {
            foreach (var (userId, round) in _round)
            {
                var friendlyFireDamage = Math.Max(0, round.FriendlyFireDamage - round.PersistedFriendlyFireDamage);
                var requisitionOrders = Math.Max(0, round.RequisitionOrders - round.PersistedRequisitionOrders);
                var xenoEvolutions = Math.Max(0, round.XenoEvolutions - round.PersistedXenoEvolutions);
                var officerWins = Math.Max(0, round.OfficerWins - round.PersistedOfficerWins);
                var queenKills = Math.Max(0, round.QueenKills - round.PersistedQueenKills);
                var queenWins = Math.Max(0, round.QueenWins - round.PersistedQueenWins);
                var queenKillParticipations = Math.Max(0, round.QueenKillParticipations - round.PersistedQueenKillParticipations);

                var hasProgress = friendlyFireDamage > 0 ||
                                  requisitionOrders > 0 ||
                                  xenoEvolutions > 0 ||
                                  officerWins > 0 ||
                                  queenKills > 0 ||
                                  queenWins > 0 ||
                                  queenKillParticipations > 0;

                if (!hasProgress)
                    continue;

                await _db.AdjustCCMPlayerAchievementStats(
                    userId.UserId,
                    friendlyFireDamage,
                    requisitionOrders,
                    xenoEvolutions,
                    officerWins,
                    queenKills,
                    queenWins,
                    queenKillParticipations);

                round.PersistedFriendlyFireDamage += friendlyFireDamage;
                round.PersistedRequisitionOrders += requisitionOrders;
                round.PersistedXenoEvolutions += xenoEvolutions;
                round.PersistedOfficerWins += officerWins;
                round.PersistedQueenKills += queenKills;
                round.PersistedQueenWins += queenWins;
                round.PersistedQueenKillParticipations += queenKillParticipations;
            }
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"FlushLiveProgressAsync failed: {e}");
        }
        finally
        {
            _flushingLiveProgress = false;
        }
    }

    private async Task<CachedAchievementState?> EnsureStateLoadedAsync(NetUserId userId)
    {
        if (!_cache.TryGetValue(userId, out var state))
        {
            state = new CachedAchievementState();
            _cache[userId] = state;
        }

        if (state.Loaded)
            return state;

        if (state.Loading)
            return null;

        state.Loading = true;
        try
        {
            Logger.InfoS(LogCategory, $"Loading achievement state for user {userId}.");
            state.BaseStats = await _db.GetCCMPlayerStats(userId.UserId);
            state.BaseSpecialStats = await _db.GetCCMPlayerAchievementStats(userId.UserId);
            state.UnlockedIds = new HashSet<string>(state.BaseSpecialStats.UnlockedIds);
            state.Loaded = true;
            await SyncUnlocksAsync(userId, state, notify: false);
            Logger.InfoS(LogCategory, $"Achievement state loaded for user {userId}. Unlocked count: {state.UnlockedIds.Count}.");
            return state;
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"EnsureStateLoadedAsync failed for user {userId}: {e}");
            return null;
        }
        finally
        {
            state.Loading = false;
        }
    }

    private async Task EvaluatePlayerAsync(NetUserId userId, bool notify, bool pushSnapshot = false)
    {
        try
        {
            var state = await EnsureStateLoadedAsync(userId);
            if (state == null)
                return;

            await SyncUnlocksAsync(userId, state, notify);

            if (pushSnapshot)
                await PushSnapshotAsync(userId, state);
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"EvaluatePlayerAsync failed for user {userId}: {e}");
        }
    }

    private async Task SyncUnlocksAsync(NetUserId userId, CachedAchievementState state, bool notify)
    {
        var context = BuildContext(userId, state);
        var unlockedNow = new List<CCMAchievementProgressData>();

        foreach (var def in Definitions)
        {
            if (def.ManualOnly)
                continue;

            var progress = Math.Clamp(def.GetProgress(context), 0, def.Goal);
            if (progress < def.Goal || !state.UnlockedIds.Add(def.Id))
                continue;

            unlockedNow.Add(def.ToProgress(progress, true));
        }

        if (unlockedNow.Count == 0)
            return;

        await PersistUnlockedIdsAsync(userId, state);

        if (!notify || !_players.TryGetSessionById(userId, out var session))
            return;

        var completedCount = CountCompleted(BuildContext(userId, state), state.UnlockedIds);
        foreach (var unlocked in unlockedNow)
        {
            RaiseNetworkEvent(
                new CCMAchievementUnlockedEvent(unlocked, completedCount, Definitions.Count),
                session.Channel);
        }
    }

    private CCMAchievementsSnapshot BuildSnapshot(NetUserId userId, CachedAchievementState state)
    {
        var context = BuildContext(userId, state);
        var achievements = Definitions
            .Select(def =>
            {
                var completed = IsCompleted(def, context, state.UnlockedIds);
                var progress = GetDisplayProgress(def, context, state.UnlockedIds);
                return def.ToProgress(progress, completed);
            })
            .ToArray();

        return new CCMAchievementsSnapshot(
            CountCompleted(context, state.UnlockedIds),
            achievements.Length,
            achievements);
    }

    private CCMAchievementProgressContext BuildContext(NetUserId userId, CachedAchievementState state)
    {
        var round = GetOrCreateRoundState(userId);
        var marineParticipated = round.MarineParticipated;
        var xenoParticipated = round.XenoParticipated;

        if (!_stats.TryGetLiveAchievementState(
                userId,
                out var live,
                out var statsMarineParticipated,
                out var statsXenoParticipated))
        {
            live = default;
        }
        else
        {
            marineParticipated |= statsMarineParticipated;
            xenoParticipated |= statsXenoParticipated;
        }

        var effectiveSpecial = new CCMPlayerAchievementStatsSnapshot(
            state.BaseSpecialStats.FriendlyFireDamage + round.FriendlyFireDamage,
            state.BaseSpecialStats.RequisitionOrders + round.RequisitionOrders,
            state.BaseSpecialStats.XenoEvolutions + round.XenoEvolutions,
            state.BaseSpecialStats.OfficerWins + round.OfficerWins,
            state.BaseSpecialStats.QueenKills + round.QueenKills,
            state.BaseSpecialStats.QueenWins + round.QueenWins,
            state.BaseSpecialStats.QueenKillParticipations + round.QueenKillParticipations,
            state.UnlockedIds.ToArray());

        return new CCMAchievementProgressContext(
            state.BaseStats,
            live,
            effectiveSpecial,
            _roundFinalized,
            _winningSide,
            marineParticipated,
            xenoParticipated);
    }

    private static int CountCompleted(CCMAchievementProgressContext context, HashSet<string> unlockedIds)
    {
        return Definitions.Count(def => IsCompleted(def, context, unlockedIds));
    }

    private static bool IsCompleted(
        CCMAchievementDefinition definition,
        CCMAchievementProgressContext context,
        HashSet<string> unlockedIds)
    {
        return unlockedIds.Contains(definition.Id) ||
               (!definition.ManualOnly && definition.GetProgress(context) >= definition.Goal);
    }

    private static int GetDisplayProgress(
        CCMAchievementDefinition definition,
        CCMAchievementProgressContext context,
        HashSet<string> unlockedIds)
    {
        var progress = Math.Clamp(definition.GetProgress(context), 0, definition.Goal);
        return unlockedIds.Contains(definition.Id) ? definition.Goal : progress;
    }

    private RoundAchievementState GetOrCreateRoundState(NetUserId userId)
    {
        if (_round.TryGetValue(userId, out var state))
            return state;

        state = new RoundAchievementState();
        _round[userId] = state;
        return state;
    }

    private static string SerializeUnlockedIds(HashSet<string> unlockedIds)
    {
        return string.Join(',', unlockedIds.OrderBy(id => id));
    }

    private async Task PersistUnlockedIdsAsync(NetUserId userId, CachedAchievementState state)
    {
        var unlocked = state.UnlockedIds.OrderBy(id => id).ToArray();
        Logger.InfoS(LogCategory, $"Persisting {unlocked.Length} unlocked achievements for user {userId}.");
        await _db.SetCCMUnlockedAchievementIds(userId.UserId, string.Join(',', unlocked));
        state.BaseSpecialStats = ReplaceUnlockedIds(state.BaseSpecialStats, unlocked);
    }

    private async Task PushSnapshotAsync(NetUserId userId, CachedAchievementState state)
    {
        if (!_players.TryGetSessionById(userId, out var session))
            return;

        RaiseNetworkEvent(new CCMAchievementsResponseEvent(BuildSnapshot(userId, state)), session.Channel);
        await Task.CompletedTask;
    }

    private static CCMPlayerAchievementStatsSnapshot ReplaceUnlockedIds(
        CCMPlayerAchievementStatsSnapshot snapshot,
        string[] unlockedIds)
    {
        return new CCMPlayerAchievementStatsSnapshot(
            snapshot.FriendlyFireDamage,
            snapshot.RequisitionOrders,
            snapshot.XenoEvolutions,
            snapshot.OfficerWins,
            snapshot.QueenKills,
            snapshot.QueenWins,
            snapshot.QueenKillParticipations,
            unlockedIds);
    }

    private bool TryGetWinningSide(out CCMStatsSide side)
    {
        side = CCMStatsSide.None;

        var query = EntityQueryEnumerator<ActiveGameRuleComponent, CMDistressSignalRuleComponent>();
        while (query.MoveNext(out _, out _, out var distress))
        {
            switch (distress.Result)
            {
                case DistressSignalRuleResult.MajorMarineVictory:
                case DistressSignalRuleResult.MinorMarineVictory:
                    side = CCMStatsSide.Marines;
                    return true;
                case DistressSignalRuleResult.MajorXenoVictory:
                case DistressSignalRuleResult.MinorXenoVictory:
                    side = CCMStatsSide.Xenos;
                    return true;
            }
        }

        return false;
    }

    private static int GetPositiveDamage(DamageChangedEvent args)
    {
        if (args.DamageDelta == null || !args.DamageIncreased)
            return 0;

        var total = args.DamageDelta.GetTotal().Float();
        return total > 0 ? (int) MathF.Round(total) : 0;
    }

    private static int GetPositiveHealing(DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageIncreased)
            return 0;

        var total = args.DamageDelta.GetTotal().Float();
        return total < 0 ? (int) MathF.Round(-total) : 0;
    }

    private static HashSet<NetUserId> GetKillParticipantPlayers(KillReportedEvent args)
    {
        var participants = new HashSet<NetUserId>();

        if (args.Primary is KillPlayerSource primary)
            participants.Add(primary.PlayerId);

        if (args.Assist is KillPlayerSource assist)
            participants.Add(assist.PlayerId);

        return participants;
    }

    private bool TryGetSourcePlayerAndSide(EntityUid? origin, EntityUid? tool, out NetUserId userId, out CCMStatsSide side)
    {
        if (TryResolvePlayerAndSide(origin, out userId, out side))
            return true;

        return TryResolvePlayerAndSide(tool, out userId, out side);
    }

    private bool TryResolvePlayerAndSide(EntityUid? entity, out NetUserId userId, out CCMStatsSide side)
    {
        userId = default;
        side = CCMStatsSide.None;

        if (entity == null)
            return false;

        var visited = new HashSet<EntityUid>();
        if (!TryResolvePlayerAndSide(entity.Value, visited, ref userId, ref side))
            return false;

        var round = GetOrCreateRoundState(userId);
        round.MarineParticipated |= side == CCMStatsSide.Marines;
        round.XenoParticipated |= side == CCMStatsSide.Xenos;
        return true;
    }

    private bool TryResolvePlayerAndSide(EntityUid entity, HashSet<EntityUid> visited, ref NetUserId userId, ref CCMStatsSide side)
    {
        if (!visited.Add(entity))
            return false;

        var current = entity;
        for (var depth = 0; depth < 8; depth++)
        {
            if (userId == default &&
                TryComp(current, out ActorComponent? actor))
            {
                userId = actor.PlayerSession.UserId;
            }

            if (userId == default &&
                TryComp(current, out MindContainerComponent? mindContainer) &&
                mindContainer.Mind is { } mindId &&
                TryComp(mindId, out MindComponent? mind) &&
                mind.UserId is { } mindUserId)
            {
                userId = mindUserId;
            }

            if (side == CCMStatsSide.None)
            {
                side = GetSide(current);
            }

            if (userId == default &&
                TryComp(current, out VehicleWeaponsComponent? vehicleWeapons) &&
                vehicleWeapons.Operator is { } weaponOperator &&
                weaponOperator != current &&
                TryResolvePlayerAndSide(weaponOperator, visited, ref userId, ref side))
            {
                return true;
            }

            if (userId == default &&
                TryComp(current, out VehicleComponent? vehicle) &&
                vehicle.Operator is { } vehicleOperator &&
                vehicleOperator != current &&
                TryResolvePlayerAndSide(vehicleOperator, visited, ref userId, ref side))
            {
                return true;
            }

            if (userId != default && side != CCMStatsSide.None)
                return true;

            if (!TryComp(current, out TransformComponent? xform) ||
                xform.ParentUid == EntityUid.Invalid ||
                xform.ParentUid == current)
            {
                break;
            }

            current = xform.ParentUid;
        }

        if (TryComp(entity, out ProjectileComponent? projectile))
        {
            if (projectile.Shooter is { } shooter &&
                shooter != entity &&
                TryResolvePlayerAndSide(shooter, visited, ref userId, ref side))
            {
                return true;
            }

            if (projectile.Weapon is { } weapon &&
                weapon != entity &&
                TryResolvePlayerAndSide(weapon, visited, ref userId, ref side))
            {
                return true;
            }
        }

        if (userId != default && side == CCMStatsSide.None)
            side = GetPlayerCurrentSide(userId);

        return userId != default && side != CCMStatsSide.None;
    }

    private EntityUid ResolveStatsTarget(EntityUid target)
    {
        var current = target;
        var visited = new HashSet<EntityUid>();

        for (var depth = 0; depth < 8 && visited.Add(current); depth++)
        {
            if (GetSide(current) != CCMStatsSide.None)
                return current;

            if (TryComp(current, out BodyPartComponent? bodyPart) &&
                bodyPart.Body is { } body)
            {
                current = body;
                continue;
            }

            if (TryComp(current, out OrganComponent? organ) &&
                organ.Body is { } organBody)
            {
                current = organBody;
                continue;
            }

            if (!TryComp(current, out TransformComponent? xform) ||
                xform.ParentUid == EntityUid.Invalid ||
                xform.ParentUid == current)
            {
                break;
            }

            current = xform.ParentUid;
        }

        return current;
    }

    private CCMStatsSide GetSide(EntityUid uid)
    {
        if (HasComp<MarineComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<XenoComponent>(uid))
            return CCMStatsSide.Xenos;
        if (HasComp<GhostComponent>(uid))
            return CCMStatsSide.None;
        return CCMStatsSide.None;
    }

    private CCMStatsSide GetPlayerCurrentSide(NetUserId player)
    {
        if (_players.TryGetSessionById(player, out var session) &&
            session.AttachedEntity is { } attached)
        {
            var side = GetSide(attached);
            if (side != CCMStatsSide.None)
                return side;
        }

        if (_round.TryGetValue(player, out var round))
        {
            if (round.MarineParticipated)
                return CCMStatsSide.Marines;
            if (round.XenoParticipated)
                return CCMStatsSide.Xenos;
        }

        return CCMStatsSide.None;
    }

    private sealed class CachedAchievementState
    {
        public CCMPlayerStatsSnapshot BaseStats = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        public CCMPlayerAchievementStatsSnapshot BaseSpecialStats = new(0, 0, 0, 0, 0, 0, 0, Array.Empty<string>());
        public HashSet<string> UnlockedIds = new();
        public bool Loaded;
        public bool Loading;
    }

    private sealed class RoundAchievementState
    {
        public bool MarineParticipated;
        public bool XenoParticipated;
        public bool OfficerParticipated;
        public bool QueenParticipated;
        public int FriendlyFireDamage;
        public int RequisitionOrders;
        public int XenoEvolutions;
        public int OfficerWins;
        public int QueenKills;
        public int QueenWins;
        public int QueenKillParticipations;
        public int PersistedFriendlyFireDamage;
        public int PersistedRequisitionOrders;
        public int PersistedXenoEvolutions;
        public int PersistedOfficerWins;
        public int PersistedQueenKills;
        public int PersistedQueenWins;
        public int PersistedQueenKillParticipations;

        public bool HasAnyProgress =>
            FriendlyFireDamage > 0 ||
            RequisitionOrders > 0 ||
            XenoEvolutions > 0 ||
            OfficerWins > 0 ||
            QueenKills > 0 ||
            QueenWins > 0 ||
            QueenKillParticipations > 0;
    }

    private readonly record struct CCMAchievementDefinition(
        string Id,
        CCMAchievementCategory Category,
        string TitleKey,
        string DescriptionKey,
        int Goal,
        Func<CCMAchievementProgressContext, int> GetProgress,
        bool ManualOnly = false)
    {
        public CCMAchievementProgressData ToProgress(int progress, bool completed)
        {
            return new CCMAchievementProgressData(Id, Category, TitleKey, DescriptionKey, progress, Goal, completed);
        }
    }

    private readonly record struct CCMAchievementProgressContext(
        CCMPlayerStatsSnapshot BaseStats,
        CCMLiveAchievementMetrics LiveStats,
        CCMPlayerAchievementStatsSnapshot Special,
        bool RoundFinalized,
        CCMStatsSide WinningSide,
        bool MarineParticipated,
        bool XenoParticipated)
    {
        public int RoundsPlayed => BaseStats.RoundsPlayed + (RoundFinalized && (MarineParticipated || XenoParticipated) ? 1 : 0);
        public int RoundsWon => BaseStats.RoundsWon +
                                (RoundFinalized && ((WinningSide == CCMStatsSide.Marines && MarineParticipated) ||
                                                    (WinningSide == CCMStatsSide.Xenos && XenoParticipated))
                                    ? 1
                                    : 0);
        public int MarineRoundsWon => BaseStats.MarineRoundsWon + (RoundFinalized && WinningSide == CCMStatsSide.Marines && MarineParticipated ? 1 : 0);
        public int XenoRoundsWon => BaseStats.XenoRoundsWon + (RoundFinalized && WinningSide == CCMStatsSide.Xenos && XenoParticipated ? 1 : 0);
        public int MarineHealingDone => BaseStats.MarineHealingDone + LiveStats.MarineHealingDone;
        public int MarineRevives => BaseStats.MarineRevives + LiveStats.MarineRevives;
        public int StructuresBuilt => BaseStats.StructuresBuilt + LiveStats.TotalStructuresBuilt;
        public int MarineStructuresBuilt => BaseStats.MarineStructuresBuilt + LiveStats.MarineStructuresBuilt;
        public int XenoStructuresBuilt => BaseStats.XenoStructuresBuilt + LiveStats.XenoStructuresBuilt;
        public int MarineKills => BaseStats.MarineKills + LiveStats.MarineKills;
        public int XenoKills => BaseStats.XenoKills + LiveStats.XenoKills;
    }
}
