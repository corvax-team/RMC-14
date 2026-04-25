// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Server._CCM.RoundEnd;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.KillTracking;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.Construction;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Construction.Events;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;
using Robust.Shared.Log;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Stats;

public sealed class CCMStatsSystem : EntitySystem
{
    private const int LeaderboardPageSize = 10;
    private const int RoundStartWinPoints = 20;
    private const int LateJoinWinPoints = 10;
    private const int GhostWinPoints = 5;
    private const float LiveProgressFlushIntervalSeconds = 10f;
    private const float DamageImpactFactor = 0.02f;
    private const float HealingImpactFactor = 0.03f;
    private const int KillImpactPoints = 5;
    private const int ReviveImpactPoints = 3;
    private const float StructureImpactPoints = 0.5f;

    [Dependency] private readonly CCMRoundWinTrackerSystem _campaignScore = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private readonly Dictionary<NetUserId, RoundPlayerStats> _roundStats = new();
    private bool _roundFinalized;
    private bool _flushingLiveProgress;
    private float _liveProgressFlushAccumulator;

    public bool TryGetLiveAchievementMetrics(NetUserId player, out CCMLiveAchievementMetrics metrics)
    {
        metrics = default;

        if (!_roundStats.TryGetValue(player, out var stats))
            return false;

        metrics = new CCMLiveAchievementMetrics(
            (int) MathF.Round(stats.MarineDamage + stats.XenoDamage),
            stats.MarineKills + stats.XenoKills,
            stats.MarineRevives,
            stats.MarineHealingDone + stats.XenoHealingDone,
            stats.MarineStructuresBuilt + stats.XenoStructuresBuilt,
            (int) MathF.Round(stats.MarineDamage),
            stats.MarineKills,
            stats.MarineRevives,
            stats.MarineHealingDone,
            stats.MarineStructuresBuilt,
            (int) MathF.Round(stats.XenoDamage),
            stats.XenoKills,
            stats.XenoHealingDone,
            stats.XenoStructuresBuilt);
        return true;
    }

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestCCMPlayerStatsEvent>(OnRequestPlayerStats);
        SubscribeNetworkEvent<RequestCCMLeaderboardEvent>(OnRequestLeaderboard);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
        SubscribeLocalEvent<GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<ProjectileComponent, ProjectileShotEvent>(OnProjectileShot);
        SubscribeLocalEvent<TargetDefibrillatedEvent>(OnTargetDefibrillated);
        SubscribeLocalEvent<RMCConstructionBuildDoAfterEvent>(OnMarineConstructionBuilt,
            after: [typeof(Content.Shared._RMC14.Construction.RMCConstructionSystem)]);
        SubscribeLocalEvent<XenoSecreteStructureDoAfterEvent>(OnXenoStructureSecreted,
            after: [typeof(Content.Shared._RMC14.Xenonids.Construction.SharedXenoConstructionSystem)]);
        SubscribeLocalEvent<XenoConstructionAddPlasmaDoAfterEvent>(OnXenoConstructionCompleted,
            after: [typeof(Content.Shared._RMC14.Xenonids.Construction.SharedXenoConstructionSystem)]);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend,
            after: [typeof(Content.Server._RMC14.Rules.DistressSignal.CMDistressSignalRuleSystem), typeof(CCMRoundWinTrackerSystem)]);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_roundFinalized || _flushingLiveProgress || _roundStats.Count == 0)
            return;

        _liveProgressFlushAccumulator += frameTime;
        if (_liveProgressFlushAccumulator < LiveProgressFlushIntervalSeconds)
            return;

        _liveProgressFlushAccumulator = 0f;
        _ = FlushLiveProgressAsync();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _roundStats.Clear();
        _roundFinalized = false;
        _flushingLiveProgress = false;
        _liveProgressFlushAccumulator = 0f;
    }

    private async void OnRequestPlayerStats(RequestCCMPlayerStatsEvent msg, EntitySessionEventArgs args)
    {
        var snapshot = await _db.GetCCMPlayerStats(args.SenderSession.UserId.UserId);
        RaiseNetworkEvent(new CCMPlayerStatsResponseEvent(snapshot), args.SenderSession.Channel);
    }

    private async void OnRequestLeaderboard(RequestCCMLeaderboardEvent msg, EntitySessionEventArgs args)
    {
        var page = await _db.GetCCMLeaderboard(
            args.SenderSession.UserId.UserId,
            msg.Category,
            msg.Timeframe,
            msg.Page,
            LeaderboardPageSize);

        RaiseNetworkEvent(new CCMLeaderboardResponseEvent(page), args.SenderSession.Channel);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (_ticker.RunLevel != GameRunLevel.InRound)
            return;

        if (HasComp<MarineComponent>(ev.Mob))
            MarkParticipation(ev.Player.UserId, CCMStatsSide.Marines, !ev.LateJoin);
        else if (HasComp<XenoComponent>(ev.Mob))
            MarkParticipation(ev.Player.UserId, CCMStatsSide.Xenos, !ev.LateJoin);

        StartActiveParticipation(ev.Player.UserId, ev.Mob);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (_ticker.RunLevel != GameRunLevel.InRound)
            return;

        var side = GetSide(ev.Entity);
        if (side != CCMStatsSide.None)
            MarkParticipation(ev.Player.UserId, side, roundStart: false);

        StartActiveParticipation(ev.Player.UserId, ev.Entity);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        if (_ticker.RunLevel != GameRunLevel.InRound)
            return;

        var stats = GetOrCreateRoundStats(ev.Player.UserId);
        StopActiveParticipation(stats);
    }

    private void OnDamageChanged(DamageChangedEvent args)
    {
        var target = args.Damageable.Owner;

        if (HasComp<XenoComponent>(target))
        {
            OnXenoDamaged(target, args);
            return;
        }

        if (HasComp<MarineComponent>(target))
            OnMarineDamaged(target, args);
    }

    private void OnGunShot(ref GunShotEvent args)
    {
        var side = GetSide(args.User);
        if (side == CCMStatsSide.None || !TryGetEntityStats(args.User, side, out var stats))
            return;

        var fired = Math.Max(1, args.Ammo.Count);
        if (side == CCMStatsSide.Marines)
            stats.MarineShotsFired += fired;
        else if (side == CCMStatsSide.Xenos)
            stats.XenoShotsFired += fired;
    }

    private void OnXenoDamaged(EntityUid target, DamageChangedEvent args)
    {
        if (TryGetSourceStats(args.Origin, args.Tool, CCMStatsSide.Xenos, out var healerStats) &&
            args.Origin != target)
        {
            var healing = GetPositiveHealing(args);
            if (healing > 0)
            {
                healerStats.XenoHealingDone += healing;
                healerStats.XenoImpact += healing * HealingImpactFactor;
                return;
            }
        }

        if (!TryGetSourceStats(args.Origin, args.Tool, CCMStatsSide.Marines, out var stats))
            return;

        var damage = GetPositiveDamage(args);
        if (damage <= 0)
            return;

        stats.MarineDamage += damage;
        stats.MarineImpact += damage * DamageImpactFactor;
    }

    private void OnMarineDamaged(EntityUid target, DamageChangedEvent args)
    {
        if (TryGetSourceStats(args.Origin, args.Tool, CCMStatsSide.Marines, out var healerStats) &&
            args.Origin != target)
        {
            var healing = GetPositiveHealing(args);
            if (healing > 0)
            {
                healerStats.MarineHealingDone += healing;
                healerStats.MarineImpact += healing * HealingImpactFactor;
                return;
            }
        }

        if (!TryGetSourceStats(args.Origin, args.Tool, CCMStatsSide.Xenos, out var stats))
            return;

        var damage = GetPositiveDamage(args);
        if (damage <= 0)
            return;

        stats.XenoDamage += damage;
        stats.XenoImpact += damage * DamageImpactFactor;
    }

    private void OnKillReported(ref KillReportedEvent args)
    {
        if (HasComp<XenoComponent>(args.Entity))
        {
            if (TryComp(args.Entity, out ActorComponent? actor))
                GetOrCreateRoundStats(actor.PlayerSession.UserId).XenoDeaths += 1;
        }
        else if (HasComp<MarineComponent>(args.Entity))
        {
            if (TryComp(args.Entity, out ActorComponent? actor))
                GetOrCreateRoundStats(actor.PlayerSession.UserId).MarineDeaths += 1;
        }

        if (args.Primary is not KillPlayerSource player || args.Suicide)
            return;

        var stats = GetOrCreateRoundStats(player.PlayerId);

        if (HasComp<XenoComponent>(args.Entity))
        {
            stats.MarineKills += 1;
            stats.MarineImpact += KillImpactPoints;
        }
        else if (HasComp<MarineComponent>(args.Entity))
        {
            stats.XenoKills += 1;
            stats.XenoImpact += KillImpactPoints;
        }
    }

    private void OnProjectileShot(Entity<ProjectileComponent> ent, ref ProjectileShotEvent args)
    {
        if (args.Shooter is not { } shooter)
            return;

        var side = GetSide(shooter);
        if (side != CCMStatsSide.Xenos || !TryGetEntityStats(shooter, side, out var stats))
            return;

        stats.XenoShotsFired += 1;
    }

    private void OnTargetDefibrillated(ref TargetDefibrillatedEvent ev)
    {
        if (!TryGetEntityStats(ev.User, CCMStatsSide.Marines, out var stats))
            return;

        stats.MarineRevives += 1;
        stats.MarineImpact += ReviveImpactPoints;
    }

    private void OnMarineConstructionBuilt(RMCConstructionBuildDoAfterEvent args)
    {
        if (args.Cancelled || !TryGetEntityStats(args.User, CCMStatsSide.Marines, out var stats))
            return;

        var count = Math.Max(1, args.Amount);
        stats.MarineStructuresBuilt += count;
        stats.MarineImpact += StructureImpactPoints * count;
    }

    private void OnXenoStructureSecreted(XenoSecreteStructureDoAfterEvent args)
    {
        if (args.Cancelled || !TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        stats.XenoStructuresBuilt += 1;
        stats.XenoImpact += StructureImpactPoints;
    }

    private void OnXenoConstructionCompleted(XenoConstructionAddPlasmaDoAfterEvent args)
    {
        if (args.Cancelled || !TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        stats.XenoStructuresBuilt += 1;
        stats.XenoImpact += StructureImpactPoints;
    }

    private async void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (_roundFinalized)
            return;

        _roundFinalized = true;

        try
        {
            await FinalizeRoundAsync();
        }
        catch (Exception e)
        {
            Log.Error($"Failed to finalize CCM round stats:\n{e}");
        }
    }

    private async Task FinalizeRoundAsync()
    {
        if (!TryGetWinningSide(out var winningSide))
            return;

        foreach (var stats in _roundStats.Values)
        {
            StopActiveParticipation(stats);
            ComputeRoundOutcome(stats, winningSide);
        }

        CCMRoundMvpData? marineMvp = null;
        CCMRoundMvpData? xenoMvp = null;

        if (winningSide == CCMStatsSide.Marines)
            marineMvp = BuildMvp(CCMStatsSide.Marines);
        else if (winningSide == CCMStatsSide.Xenos)
            xenoMvp = BuildMvp(CCMStatsSide.Xenos);

        SendRoundEndStats(winningSide, marineMvp, xenoMvp);
        await PersistRoundStatsAsync();
    }

    private async Task PersistRoundStatsAsync()
    {
        var now = DateTime.UtcNow;
        var saveTasks = new List<Task>();

        foreach (var pair in _roundStats)
        {
            var player = pair.Key.UserId;
            var stats = pair.Value;
            if (!stats.HadAnyParticipation)
                continue;

            var marineKills = Math.Max(0, stats.MarineKills - stats.PersistedMarineKills);
            var xenoKills = Math.Max(0, stats.XenoKills - stats.PersistedXenoKills);
            var marineRevives = Math.Max(0, stats.MarineRevives - stats.PersistedMarineRevives);
            var marineHealingDone = Math.Max(0, stats.MarineHealingDone - stats.PersistedMarineHealingDone);
            var xenoHealingDone = Math.Max(0, stats.XenoHealingDone - stats.PersistedXenoHealingDone);
            var marineStructuresBuilt = Math.Max(0, stats.MarineStructuresBuilt - stats.PersistedMarineStructuresBuilt);
            var xenoStructuresBuilt = Math.Max(0, stats.XenoStructuresBuilt - stats.PersistedXenoStructuresBuilt);
            var totalKills = marineKills + xenoKills;
            var totalRevives = marineRevives;
            var totalHealingDone = marineHealingDone + xenoHealingDone;
            var totalStructuresBuilt = marineStructuresBuilt + xenoStructuresBuilt;

            saveTasks.Add(_db.SaveCCMRoundStats(
                player,
                now.Year,
                now.Month,
                stats.GeneralRoundsPlayed,
                stats.GeneralRoundsWon,
                stats.GeneralRoundsLost,
                (int) stats.RoundSecondsPlayed,
                (int) MathF.Round(stats.TotalDamage),
                totalKills,
                stats.VictoryPointsEarned,
                stats.TotalImpactPoints,
                totalRevives,
                totalHealingDone,
                totalStructuresBuilt,
                stats.TotalDeaths,
                stats.TotalShotsFired,
                stats.MarineRoundsPlayed,
                stats.MarineRoundsWon,
                stats.MarineRoundsLost,
                (int) MathF.Round(stats.MarineDamage),
                marineKills,
                stats.MarineVictoryPointsEarned,
                stats.MarineImpactPoints,
                marineRevives,
                marineHealingDone,
                marineStructuresBuilt,
                stats.MarineDeaths,
                stats.MarineShotsFired,
                stats.XenoRoundsPlayed,
                stats.XenoRoundsWon,
                stats.XenoRoundsLost,
                (int) MathF.Round(stats.XenoDamage),
                xenoKills,
                stats.XenoVictoryPointsEarned,
                stats.XenoImpactPoints,
                xenoHealingDone,
                xenoStructuresBuilt,
                stats.XenoDeaths,
                stats.XenoShotsFired));
        }

        if (saveTasks.Count > 0)
            await Task.WhenAll(saveTasks);
    }

    private async Task FlushLiveProgressAsync()
    {
        if (_flushingLiveProgress || _roundFinalized)
            return;

        _flushingLiveProgress = true;

        try
        {
            var now = DateTime.UtcNow;
            var saveTasks = new List<Task>();

            foreach (var pair in _roundStats)
            {
                var player = pair.Key.UserId;
                var stats = pair.Value;

                var marineKills = Math.Max(0, stats.MarineKills - stats.PersistedMarineKills);
                var xenoKills = Math.Max(0, stats.XenoKills - stats.PersistedXenoKills);
                var marineRevives = Math.Max(0, stats.MarineRevives - stats.PersistedMarineRevives);
                var marineHealingDone = Math.Max(0, stats.MarineHealingDone - stats.PersistedMarineHealingDone);
                var xenoHealingDone = Math.Max(0, stats.XenoHealingDone - stats.PersistedXenoHealingDone);
                var marineStructuresBuilt = Math.Max(0, stats.MarineStructuresBuilt - stats.PersistedMarineStructuresBuilt);
                var xenoStructuresBuilt = Math.Max(0, stats.XenoStructuresBuilt - stats.PersistedXenoStructuresBuilt);

                var hasProgress = marineKills > 0 ||
                                  xenoKills > 0 ||
                                  marineRevives > 0 ||
                                  marineHealingDone > 0 ||
                                  xenoHealingDone > 0 ||
                                  marineStructuresBuilt > 0 ||
                                  xenoStructuresBuilt > 0;

                if (!hasProgress)
                    continue;

                saveTasks.Add(_db.SaveCCMRoundStats(
                    player,
                    now.Year,
                    now.Month,
                    0,
                    0,
                    0,
                    0,
                    0,
                    marineKills + xenoKills,
                    0,
                    0,
                    marineRevives,
                    marineHealingDone + xenoHealingDone,
                    marineStructuresBuilt + xenoStructuresBuilt,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    marineKills,
                    0,
                    0,
                    marineRevives,
                    marineHealingDone,
                    marineStructuresBuilt,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    xenoKills,
                    0,
                    0,
                    xenoHealingDone,
                    xenoStructuresBuilt,
                    0,
                    0));

                stats.PersistedMarineKills += marineKills;
                stats.PersistedXenoKills += xenoKills;
                stats.PersistedMarineRevives += marineRevives;
                stats.PersistedMarineHealingDone += marineHealingDone;
                stats.PersistedXenoHealingDone += xenoHealingDone;
                stats.PersistedMarineStructuresBuilt += marineStructuresBuilt;
                stats.PersistedXenoStructuresBuilt += xenoStructuresBuilt;
            }

            if (saveTasks.Count > 0)
                await Task.WhenAll(saveTasks);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to flush live CCM progress:\n{e}");
        }
        finally
        {
            _flushingLiveProgress = false;
        }
    }

    private void SendRoundEndStats(CCMStatsSide winningSide, CCMRoundMvpData? marineMvp, CCMRoundMvpData? xenoMvp)
    {
        foreach (var session in _players.Sessions)
        {
            var personalStats = _roundStats.TryGetValue(session.UserId, out var stats)
                ? BuildPersonalStats(stats)
                : null;
            var score = personalStats?.RoundScore ?? 0;
            RaiseNetworkEvent(
                new CCMRoundEndStatsEvent(
                    score,
                    _campaignScore.MarineWins,
                    _campaignScore.XenoWins,
                    winningSide,
                    personalStats,
                    marineMvp,
                    xenoMvp),
                session.Channel);
        }
    }

    private CCMRoundMvpData? BuildMvp(CCMStatsSide side)
    {
        var best = _roundStats
            .Where(p => side == CCMStatsSide.Marines ? p.Value.MarineParticipated : p.Value.XenoParticipated)
            .OrderByDescending(p => side == CCMStatsSide.Marines ? p.Value.MarineImpactPoints : p.Value.XenoImpactPoints)
            .FirstOrDefault();

        if (best.Key == default)
            return null;

        var impact = side == CCMStatsSide.Marines ? best.Value.MarineImpactPoints : best.Value.XenoImpactPoints;
        if (impact <= 0)
            return null;

        var ckey = TryGetCurrentCkey(best.Key, out var resolvedCkey)
            ? resolvedCkey
            : best.Key.ToString();
        var name = TryGetCurrentName(best.Key, out var netEntity, out var resolvedName)
            ? resolvedName
            : ckey;

        var stats = best.Value;
        var damage = side == CCMStatsSide.Marines ? (int) MathF.Round(stats.MarineDamage) : (int) MathF.Round(stats.XenoDamage);
        var kills = side == CCMStatsSide.Marines ? stats.MarineKills : stats.XenoKills;
        var healing = side == CCMStatsSide.Marines ? stats.MarineHealingDone : stats.XenoHealingDone;
        var revives = side == CCMStatsSide.Marines ? stats.MarineRevives : 0;
        var structures = side == CCMStatsSide.Marines ? stats.MarineStructuresBuilt : stats.XenoStructuresBuilt;

        return new CCMRoundMvpData(
            name,
            ckey,
            netEntity,
            side,
            impact,
            damage,
            kills,
            healing,
            revives,
            structures);
    }

    private bool TryGetCurrentName(NetUserId userId, out NetEntity? netEntity, out string name)
    {
        netEntity = null;
        name = userId.ToString();

        if (!_players.TryGetSessionById(userId, out var session))
            return false;

        if (session.AttachedEntity is not { } attached)
            return false;

        name = MetaData(attached).EntityName;
        netEntity = GetNetEntity(attached);
        return true;
    }

    private bool TryGetCurrentCkey(NetUserId userId, out string ckey)
    {
        ckey = userId.ToString();

        if (!_players.TryGetSessionById(userId, out var session))
            return false;

        ckey = session.Name;
        return true;
    }

    private void ComputeRoundOutcome(RoundPlayerStats stats, CCMStatsSide winningSide)
    {
        stats.TotalDamage = stats.MarineDamage + stats.XenoDamage;
        stats.TotalKills = stats.MarineKills + stats.XenoKills;
        stats.TotalRevives = stats.MarineRevives;
        stats.TotalHealingDone = stats.MarineHealingDone + stats.XenoHealingDone;
        stats.TotalStructuresBuilt = stats.MarineStructuresBuilt + stats.XenoStructuresBuilt;
        stats.TotalDeaths = stats.MarineDeaths + stats.XenoDeaths;
        stats.TotalShotsFired = stats.MarineShotsFired + stats.XenoShotsFired;
        stats.MarineImpactPoints = (int) MathF.Round(stats.MarineImpact);
        stats.XenoImpactPoints = (int) MathF.Round(stats.XenoImpact);
        stats.TotalImpactPoints = stats.MarineImpactPoints + stats.XenoImpactPoints;

        var winningParticipation = winningSide == CCMStatsSide.Marines
            ? stats.MarineParticipated
            : stats.XenoParticipated;

        if (!winningParticipation && !stats.HadAnyParticipation)
            return;

        if (stats.HadAnyParticipation)
        {
            stats.GeneralRoundsPlayed = 1;
            if (winningParticipation)
                stats.GeneralRoundsWon = 1;
            else
                stats.GeneralRoundsLost = 1;
        }

        if (stats.MarineParticipated)
        {
            stats.MarineRoundsPlayed = 1;
            if (winningSide == CCMStatsSide.Marines)
                stats.MarineRoundsWon = 1;
            else
                stats.MarineRoundsLost = 1;
        }

        if (stats.XenoParticipated)
        {
            stats.XenoRoundsPlayed = 1;
            if (winningSide == CCMStatsSide.Xenos)
                stats.XenoRoundsWon = 1;
            else
                stats.XenoRoundsLost = 1;
        }

        stats.MarineVictoryPointsEarned = ComputeWinPoints(stats, CCMStatsSide.Marines, winningSide);
        stats.XenoVictoryPointsEarned = ComputeWinPoints(stats, CCMStatsSide.Xenos, winningSide);
        stats.VictoryPointsEarned = stats.MarineVictoryPointsEarned + stats.XenoVictoryPointsEarned;
        stats.RoundScoreEarned = stats.VictoryPointsEarned + stats.TotalKills;
    }

    private int ComputeWinPoints(RoundPlayerStats stats, CCMStatsSide side, CCMStatsSide winningSide)
    {
        if (side != winningSide)
            return 0;

        var participated = side == CCMStatsSide.Marines ? stats.MarineParticipated : stats.XenoParticipated;
        if (!participated)
            return 0;

        if (IsCurrentlyGhost(stats.Player))
            return GhostWinPoints;

        var roundStart = side == CCMStatsSide.Marines ? stats.MarineRoundStart : stats.XenoRoundStart;
        return roundStart ? RoundStartWinPoints : LateJoinWinPoints;
    }

    private bool IsCurrentlyGhost(NetUserId userId)
    {
        if (!_players.TryGetSessionById(userId, out var session))
            return false;

        if (session.AttachedEntity is not { } attached)
            return false;

        return HasComp<GhostComponent>(attached);
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

    private void MarkParticipation(NetUserId player, CCMStatsSide side, bool roundStart)
    {
        var stats = GetOrCreateRoundStats(player);
        if (side == CCMStatsSide.Marines)
        {
            stats.MarineParticipated = true;
            stats.MarineRoundStart |= roundStart;
            stats.MarineLateJoin |= !roundStart;
        }
        else if (side == CCMStatsSide.Xenos)
        {
            stats.XenoParticipated = true;
            stats.XenoRoundStart |= roundStart;
            stats.XenoLateJoin |= !roundStart;
        }
    }

    private void StartActiveParticipation(NetUserId player, EntityUid entity)
    {
        var stats = GetOrCreateRoundStats(player);
        StopActiveParticipation(stats);

        var side = GetSide(entity);
        if (side == CCMStatsSide.None)
            return;

        stats.ActiveSide = side;
        stats.ActiveSince = _timing.CurTime;
    }

    private void StopActiveParticipation(RoundPlayerStats stats)
    {
        if (stats.ActiveSide == CCMStatsSide.None || stats.ActiveSince == null)
            return;

        var duration = (_timing.CurTime - stats.ActiveSince.Value).TotalSeconds;
        if (duration > 0)
            stats.RoundSecondsPlayed += duration;

        stats.ActiveSide = CCMStatsSide.None;
        stats.ActiveSince = null;
    }

    private CCMStatsSide GetSide(EntityUid uid)
    {
        if (HasComp<MarineComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<XenoComponent>(uid))
            return CCMStatsSide.Xenos;
        return CCMStatsSide.None;
    }

    private CCMRoundPersonalStatsData BuildPersonalStats(RoundPlayerStats stats)
    {
        return new CCMRoundPersonalStatsData(
            stats.RoundScoreEarned,
            stats.VictoryPointsEarned,
            stats.TotalImpactPoints,
            (int) MathF.Round(stats.TotalDamage),
            stats.TotalKills,
            stats.TotalHealingDone,
            stats.TotalRevives,
            stats.TotalStructuresBuilt,
            (int) Math.Round(stats.RoundSecondsPlayed),
            stats.MarineVictoryPointsEarned,
            stats.MarineImpactPoints,
            (int) MathF.Round(stats.MarineDamage),
            stats.MarineKills,
            stats.MarineHealingDone,
            stats.MarineRevives,
            stats.MarineStructuresBuilt,
            stats.XenoVictoryPointsEarned,
            stats.XenoImpactPoints,
            (int) MathF.Round(stats.XenoDamage),
            stats.XenoKills,
            stats.XenoHealingDone,
            stats.XenoStructuresBuilt);
    }

    private bool TryGetSourceStats(EntityUid? origin, EntityUid? tool, CCMStatsSide expectedSide, out RoundPlayerStats stats)
    {
        if (TryGetEntityStats(origin, expectedSide, out stats))
            return true;

        return TryGetEntityStats(tool, expectedSide, out stats);
    }

    private bool TryGetEntityStats(EntityUid? entity, CCMStatsSide expectedSide, out RoundPlayerStats stats)
    {
        stats = default!;

        if (entity == null)
            return false;

        var current = entity.Value;
        for (var depth = 0; depth < 8; depth++)
        {
            if (TryComp(current, out ActorComponent? actor) && GetSide(current) == expectedSide)
            {
                MarkParticipation(actor.PlayerSession.UserId, expectedSide, roundStart: false);
                stats = GetOrCreateRoundStats(actor.PlayerSession.UserId);
                return true;
            }

            if (!TryComp(current, out TransformComponent? xform) ||
                xform.ParentUid == EntityUid.Invalid ||
                xform.ParentUid == current)
            {
                break;
            }

            current = xform.ParentUid;
        }

        if (TryComp(entity.Value, out ProjectileComponent? projectile) &&
            projectile.Shooter is { } shooter &&
            shooter != entity.Value)
        {
            return TryGetEntityStats(shooter, expectedSide, out stats);
        }

        return false;
    }

    private static float GetPositiveDamage(DamageChangedEvent args)
    {
        if (args.DamageDelta == null || !args.DamageIncreased)
            return 0f;

        var total = args.DamageDelta.GetTotal().Float();
        return total > 0 ? total : 0f;
    }

    private static int GetPositiveHealing(DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageIncreased)
            return 0;

        var total = args.DamageDelta.GetTotal().Float();
        if (total >= 0)
            return 0;

        return (int) MathF.Round(-total);
    }

    private RoundPlayerStats GetOrCreateRoundStats(NetUserId player)
    {
        if (_roundStats.TryGetValue(player, out var stats))
            return stats;

        stats = new RoundPlayerStats(player);
        _roundStats[player] = stats;
        return stats;
    }

    private sealed class RoundPlayerStats
    {
        public NetUserId Player { get; }

        public CCMStatsSide ActiveSide = CCMStatsSide.None;
        public TimeSpan? ActiveSince;
        public double RoundSecondsPlayed;

        public bool MarineParticipated;
        public bool MarineRoundStart;
        public bool MarineLateJoin;
        public bool XenoParticipated;
        public bool XenoRoundStart;
        public bool XenoLateJoin;

        public float MarineDamage;
        public float XenoDamage;
        public int MarineKills;
        public int XenoKills;
        public int MarineRevives;
        public int MarineHealingDone;
        public int XenoHealingDone;
        public int MarineStructuresBuilt;
        public int XenoStructuresBuilt;
        public int PersistedMarineKills;
        public int PersistedXenoKills;
        public int PersistedMarineRevives;
        public int PersistedMarineHealingDone;
        public int PersistedXenoHealingDone;
        public int PersistedMarineStructuresBuilt;
        public int PersistedXenoStructuresBuilt;
        public int MarineDeaths;
        public int XenoDeaths;
        public int MarineShotsFired;
        public int XenoShotsFired;
        public float MarineImpact;
        public float XenoImpact;

        public float TotalDamage;
        public int TotalKills;
        public int TotalRevives;
        public int TotalHealingDone;
        public int TotalStructuresBuilt;
        public int TotalDeaths;
        public int TotalShotsFired;
        public int MarineImpactPoints;
        public int XenoImpactPoints;
        public int TotalImpactPoints;
        public int MarineVictoryPointsEarned;
        public int XenoVictoryPointsEarned;
        public int VictoryPointsEarned;
        public int RoundScoreEarned;

        public int GeneralRoundsPlayed;
        public int GeneralRoundsWon;
        public int GeneralRoundsLost;
        public int MarineRoundsPlayed;
        public int MarineRoundsWon;
        public int MarineRoundsLost;
        public int XenoRoundsPlayed;
        public int XenoRoundsWon;
        public int XenoRoundsLost;

        public bool HadAnyParticipation => MarineParticipated || XenoParticipated;

        public RoundPlayerStats(NetUserId player)
        {
            Player = player;
        }
    }
}

public readonly record struct CCMLiveAchievementMetrics(
    int TotalDamage,
    int TotalKills,
    int TotalRevives,
    int TotalHealingDone,
    int TotalStructuresBuilt,
    int MarineDamage,
    int MarineKills,
    int MarineRevives,
    int MarineHealingDone,
    int MarineStructuresBuilt,
    int XenoDamage,
    int XenoKills,
    int XenoHealingDone,
    int XenoStructuresBuilt);
