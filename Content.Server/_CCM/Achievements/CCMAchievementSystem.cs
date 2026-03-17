using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.KillTracking;
using Content.Server._CCM.Stats;
using Content.Shared._CCM.Achievements;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.Construction;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Construction.Events;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Achievements;

public sealed class CCMAchievementSystem : EntitySystem
{
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
        new("general_veteran", CCMAchievementCategory.General, "ccm-achievement-general-veteran-title", "ccm-achievement-general-veteran-desc", 50, ctx => ctx.RoundsPlayed),
        new("general_living_legend", CCMAchievementCategory.General, "ccm-achievement-general-living-legend-title", "ccm-achievement-general-living-legend-desc", 200, ctx => ctx.RoundsPlayed),
        new("general_campaign_veteran", CCMAchievementCategory.General, "ccm-achievement-general-campaign-veteran-title", "ccm-achievement-general-campaign-veteran-desc", 50, ctx => ctx.RoundsWon),
        new("general_war_legend", CCMAchievementCategory.General, "ccm-achievement-general-war-legend-title", "ccm-achievement-general-war-legend-desc", 200, ctx => ctx.RoundsWon),

        new("misc_logistician", CCMAchievementCategory.Misc, "ccm-achievement-misc-logistician-title", "ccm-achievement-misc-logistician-desc", 20, ctx => ctx.Special.RequisitionOrders),
        new("misc_friendly_fire", CCMAchievementCategory.Misc, "ccm-achievement-misc-friendly-fire-title", "ccm-achievement-misc-friendly-fire-desc", 500, ctx => ctx.Special.FriendlyFireDamage),
        new("misc_queen_slayer", CCMAchievementCategory.Misc, "ccm-achievement-misc-queen-slayer-title", "ccm-achievement-misc-queen-slayer-desc", 1, ctx => ctx.Special.QueenKillParticipations),

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
    private CCMStatsSide _winningSide = CCMStatsSide.None;

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestCCMAchievementsEvent>(OnRequestAchievements);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
        SubscribeLocalEvent<TargetDefibrillatedEvent>(OnTargetDefibrillated);
        SubscribeLocalEvent<RMCConstructionBuildDoAfterEvent>(OnMarineConstructionBuilt,
            after: [typeof(Content.Shared._RMC14.Construction.RMCConstructionSystem)]);
        SubscribeLocalEvent<XenoSecreteStructureDoAfterEvent>(OnXenoStructureSecreted,
            after: [typeof(Content.Shared._RMC14.Xenonids.Construction.SharedXenoConstructionSystem)]);
        SubscribeLocalEvent<XenoConstructionAddPlasmaDoAfterEvent>(OnXenoConstructionCompleted,
            after: [typeof(Content.Shared._RMC14.Xenonids.Construction.SharedXenoConstructionSystem)]);
        SubscribeLocalEvent<NewXenoEvolvedEvent>(OnNewXenoEvolved);
        SubscribeLocalEvent<CCMRequisitionOrderedEvent>(OnRequisitionOrdered);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend,
            after: [typeof(CCMStatsSystem)]);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _round.Clear();
        _cache.Clear();
        _roundFinalized = false;
        _winningSide = CCMStatsSide.None;
    }

    private async void OnRequestAchievements(RequestCCMAchievementsEvent ev, EntitySessionEventArgs args)
    {
        var state = await EnsureStateLoadedAsync(args.SenderSession.UserId);
        if (state == null)
            return;

        var snapshot = BuildSnapshot(args.SenderSession.UserId, state);
        RaiseNetworkEvent(new CCMAchievementsResponseEvent(snapshot), args.SenderSession.Channel);
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

    private void OnDamageChanged(DamageChangedEvent args)
    {
        var damage = GetPositiveDamage(args);
        var healing = GetPositiveHealing(args);
        if (damage <= 0 && healing <= 0)
            return;

        if (!TryGetSourcePlayerAndSide(args.Origin, args.Tool, out var userId, out var sourceSide))
            return;

        if (sourceSide == CCMStatsSide.None)
            return;

        if (damage > 0 && GetSide(args.Damageable.Owner) == sourceSide && args.Origin != args.Damageable.Owner)
        {
            GetOrCreateRoundState(userId).FriendlyFireDamage += damage;
        }

        _ = EvaluatePlayerAsync(userId, notify: true);
    }

    private void OnKillReported(ref KillReportedEvent args)
    {
        if (args.Primary is not KillPlayerSource player || args.Suicide)
            return;

        var round = GetOrCreateRoundState(player.PlayerId);

        if (HasComp<XenoEvolutionGranterComponent>(args.Entity) && HasComp<XenoComponent>(args.Entity))
            round.QueenKillParticipations += 1;

        if (_players.TryGetSessionById(player.PlayerId, out var session) &&
            session.AttachedEntity is { } attached &&
            HasComp<XenoEvolutionGranterComponent>(attached) &&
            HasComp<XenoComponent>(attached) &&
            HasComp<MarineComponent>(args.Entity))
        {
            round.QueenKills += 1;
        }

        _ = EvaluatePlayerAsync(player.PlayerId, notify: true);
    }

    private void OnTargetDefibrillated(ref TargetDefibrillatedEvent ev)
    {
        if (!TryComp(ev.User, out ActorComponent? actor))
            return;

        _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true);
    }

    private void OnMarineConstructionBuilt(RMCConstructionBuildDoAfterEvent args)
    {
        if (!args.Cancelled && TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true);
    }

    private void OnXenoStructureSecreted(XenoSecreteStructureDoAfterEvent args)
    {
        if (!args.Cancelled && TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true);
    }

    private void OnXenoConstructionCompleted(XenoConstructionAddPlasmaDoAfterEvent args)
    {
        if (!args.Cancelled && TryComp(args.User, out ActorComponent? actor))
            _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true);
    }

    private void OnNewXenoEvolved(ref NewXenoEvolvedEvent args)
    {
        if (!TryComp(args.NewXeno, out ActorComponent? actor))
            return;

        var round = GetOrCreateRoundState(actor.PlayerSession.UserId);
        round.XenoEvolutions += 1;
        round.XenoParticipated = true;
        round.QueenParticipated |= HasComp<XenoEvolutionGranterComponent>(args.NewXeno);

        _ = EvaluatePlayerAsync(actor.PlayerSession.UserId, notify: true);
    }

    private void OnRequisitionOrdered(CCMRequisitionOrderedEvent ev)
    {
        GetOrCreateRoundState(ev.UserId).RequisitionOrders += 1;
        _ = EvaluatePlayerAsync(ev.UserId, notify: true);
    }

    private async void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (_roundFinalized || !TryGetWinningSide(out _winningSide))
            return;

        _roundFinalized = true;

        foreach (var (userId, round) in _round)
        {
            if (_winningSide == CCMStatsSide.Marines && round.OfficerParticipated)
                round.OfficerWins += 1;

            if (_winningSide == CCMStatsSide.Xenos && round.QueenParticipated)
                round.QueenWins += 1;

            if (!round.HasAnyProgress)
                continue;

            await _db.AdjustCCMPlayerAchievementStats(
                userId.UserId,
                round.FriendlyFireDamage,
                round.RequisitionOrders,
                round.XenoEvolutions,
                round.OfficerWins,
                round.QueenKills,
                round.QueenWins,
                round.QueenKillParticipations);
        }

        foreach (var userId in _round.Keys.ToArray())
        {
            _ = EvaluatePlayerAsync(userId, notify: true);
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
            state.BaseStats = await _db.GetCCMPlayerStats(userId.UserId);
            state.BaseSpecialStats = await _db.GetCCMPlayerAchievementStats(userId.UserId);
            state.UnlockedIds = new HashSet<string>(state.BaseSpecialStats.UnlockedIds);
            state.Loaded = true;
            await SyncUnlocksAsync(userId, state, notify: false);
            return state;
        }
        finally
        {
            state.Loading = false;
        }
    }

    private async Task EvaluatePlayerAsync(NetUserId userId, bool notify)
    {
        var state = await EnsureStateLoadedAsync(userId);
        if (state == null)
            return;

        await SyncUnlocksAsync(userId, state, notify);
    }

    private async Task SyncUnlocksAsync(NetUserId userId, CachedAchievementState state, bool notify)
    {
        var context = BuildContext(userId, state);
        var unlockedNow = new List<CCMAchievementProgressData>();

        foreach (var def in Definitions)
        {
            var progress = Math.Clamp(def.GetProgress(context), 0, def.Goal);
            if (progress < def.Goal || !state.UnlockedIds.Add(def.Id))
                continue;

            unlockedNow.Add(def.ToProgress(progress, true));
        }

        if (unlockedNow.Count == 0)
            return;

        await _db.SetCCMUnlockedAchievementIds(userId.UserId, SerializeUnlockedIds(state.UnlockedIds));

        if (!notify || !_players.TryGetSessionById(userId, out var session))
            return;

        var completedCount = CountCompleted(BuildContext(userId, state));
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
                var progress = Math.Clamp(def.GetProgress(context), 0, def.Goal);
                return def.ToProgress(progress, progress >= def.Goal);
            })
            .ToArray();

        return new CCMAchievementsSnapshot(
            achievements.Count(a => a.Completed),
            achievements.Length,
            achievements);
    }

    private CCMAchievementProgressContext BuildContext(NetUserId userId, CachedAchievementState state)
    {
        _stats.TryGetLiveAchievementMetrics(userId, out var live);
        var round = GetOrCreateRoundState(userId);

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
            round.MarineParticipated,
            round.XenoParticipated);
    }

    private static int CountCompleted(CCMAchievementProgressContext context)
    {
        return Definitions.Count(def => def.GetProgress(context) >= def.Goal);
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

    private bool TryGetSourcePlayerAndSide(EntityUid? origin, EntityUid? tool, out NetUserId userId, out CCMStatsSide side)
    {
        if (TryGetPlayerAndSide(origin, out userId, out side))
            return true;

        return TryGetPlayerAndSide(tool, out userId, out side);
    }

    private bool TryGetPlayerAndSide(EntityUid? entity, out NetUserId userId, out CCMStatsSide side)
    {
        userId = default;
        side = CCMStatsSide.None;

        if (entity == null)
            return false;

        var current = entity.Value;
        for (var depth = 0; depth < 8; depth++)
        {
            if (TryComp(current, out ActorComponent? actor))
            {
                side = GetSide(current);
                if (side != CCMStatsSide.None)
                {
                    userId = actor.PlayerSession.UserId;
                    return true;
                }
            }

            if (!TryComp(current, out TransformComponent? xform) ||
                xform.ParentUid == EntityUid.Invalid ||
                xform.ParentUid == current)
            {
                break;
            }

            current = xform.ParentUid;
        }

        return false;
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
        Func<CCMAchievementProgressContext, int> GetProgress)
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
        public int MarineStructuresBuilt => BaseStats.MarineStructuresBuilt + LiveStats.MarineStructuresBuilt;
        public int MarineKills => BaseStats.MarineKills + LiveStats.MarineKills;
        public int XenoKills => BaseStats.XenoKills + LiveStats.XenoKills;
    }
}
