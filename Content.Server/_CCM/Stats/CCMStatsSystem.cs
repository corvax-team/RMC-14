// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Server._CCM.RoundEnd;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.KillTracking;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Actions.Components;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.Construction;
using Content.Shared._RMC14.Entrenching;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Synth;
using Content.Shared._RMC14.Survivor;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Construction.Events;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
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
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared._RMC14.Vehicle;
using Content.Server._RMC14.Xenonids.Construction.ResinHole;
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
    private const float DamageImpactFactor = 0.01f;
    private const float HealingImpactFactor = 0.03f;
    private const int MarineKillImpactPoints = 5;
    private const int XenoKillImpactPoints = 3;
    private const float MarineStructureImpactPoints = 0.2f;
    private const float XenoStructureImpactPoints = 0.025f;
    private const int DamageDiagnosticsHistoryLimit = 25;

    [Dependency] private readonly CCMRoundWinTrackerSystem _campaignScore = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly KillTrackingSystem _killTracking = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private readonly Dictionary<NetUserId, RoundPlayerStats> _roundStats = new();
    private bool _roundFinalized;
    private bool _flushingLiveProgress;
    private float _liveProgressFlushAccumulator;
    private readonly HashSet<EntityUid> _countedDefibRevives = new();
    private readonly Queue<DamageDiagnosticEntry> _recentDamageDiagnostics = new();
    private float _unattributedDamageToMarines;
    private float _unattributedDamageToXenos;
    private int _unattributedHitsToMarines;
    private int _unattributedHitsToXenos;
    private float _fallbackMarineDamage;
    private float _fallbackXenoDamage;
    private int _fallbackMarineHits;
    private int _fallbackXenoHits;

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

    public bool TryGetLiveAchievementState(
        NetUserId player,
        out CCMLiveAchievementMetrics metrics,
        out bool marineParticipated,
        out bool xenoParticipated)
    {
        metrics = default;
        marineParticipated = false;
        xenoParticipated = false;

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
        marineParticipated = stats.MarineParticipated;
        xenoParticipated = stats.XenoParticipated;
        return true;
    }

    public NetUserId[] GetTrackedPlayers()
    {
        return _roundStats.Keys.ToArray();
    }

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestCCMPlayerStatsEvent>(OnRequestPlayerStats);
        SubscribeNetworkEvent<RequestCCMLeaderboardEvent>(OnRequestLeaderboard);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MarineComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<RMCSurvivorComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<SynthComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<XenoComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<BodyPartComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<OrganComponent, DamageChangedEvent>((uid, _, args) => OnDamageChanged(uid, args));
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
        SubscribeLocalEvent<TargetDefibrillatedEvent>(OnTargetDefibrillated);
        SubscribeLocalEvent<GunComponent, TakeAmmoEvent>(OnGunTakeAmmo);
        SubscribeLocalEvent<ProjectileComponent, ProjectileShotEvent>(OnProjectileShot);
        SubscribeLocalEvent<RMCStructureBuiltEvent>(OnMarineStructureBuilt);
        SubscribeLocalEvent<XenoStructureBuiltEvent>(OnXenoStructureBuilt);
        SubscribeLocalEvent<XenoStructureUpgradedEvent>(OnXenoStructureUpgraded);
        SubscribeLocalEvent<XenoResinHolePlacedEvent>(OnXenoResinHolePlaced);
        SubscribeLocalEvent<XenoTunnelPlacedEvent>(OnXenoTunnelPlaced);
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
        _countedDefibRevives.Clear();
        _recentDamageDiagnostics.Clear();
        _roundFinalized = false;
        _flushingLiveProgress = false;
        _liveProgressFlushAccumulator = 0f;
        _unattributedDamageToMarines = 0f;
        _unattributedDamageToXenos = 0f;
        _unattributedHitsToMarines = 0;
        _unattributedHitsToXenos = 0;
        _fallbackMarineDamage = 0f;
        _fallbackXenoDamage = 0f;
        _fallbackMarineHits = 0;
        _fallbackXenoHits = 0;
    }

    private async void OnRequestPlayerStats(RequestCCMPlayerStatsEvent msg, EntitySessionEventArgs args)
    {
        var snapshot = await _db.GetCCMPlayerStats(args.SenderSession.UserId.UserId);
        if (_roundStats.TryGetValue(args.SenderSession.UserId, out var liveStats))
            snapshot = MergeLiveSnapshot(snapshot, liveStats);

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
        if (!IsRoundStatsTrackingActive())
            return;

        var side = GetSide(ev.Mob);
        if (side == CCMStatsSide.Marines)
        {
            EnsureStatsKillTracker(ev.Mob);
            MarkParticipation(ev.Player.UserId, side, !ev.LateJoin);
        }
        else if (side == CCMStatsSide.Xenos)
        {
            EnsureStatsKillTracker(ev.Mob);
            MarkParticipation(ev.Player.UserId, side, !ev.LateJoin);
        }

        StartActiveParticipation(ev.Player.UserId, ev.Mob);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (!IsRoundStatsTrackingActive())
            return;

        var side = GetSide(ev.Entity);
        if (side != CCMStatsSide.None)
        {
            EnsureStatsKillTracker(ev.Entity);
            MarkParticipation(ev.Player.UserId, side, roundStart: false);
        }

        StartActiveParticipation(ev.Player.UserId, ev.Entity);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        if (!IsRoundStatsTrackingActive())
            return;

        UpdateLastKnownIdentity(ev.Player.UserId, ev.Entity);
        var stats = GetOrCreateRoundStats(ev.Player.UserId);
        StopActiveParticipation(stats);
    }

    private void OnDamageChanged(EntityUid uid, DamageChangedEvent args)
    {
        var damage = GetPositiveDamage(args);
        var healing = GetPositiveHealing(args);
        if (damage <= 0 && healing <= 0)
            return;

        var target = ResolveStatsTarget(uid);
        var side = GetSide(target);
        if (healing > 0)
            HandleHealingChanged(target, side, args, healing);

        if (damage > 0)
            HandleDamageChanged(target, side, args, damage);
    }

    private void OnGunTakeAmmo(Entity<GunComponent> gun, ref TakeAmmoEvent args)
    {
        if (!TryGetSourceStats(args.User, out var side, out var stats) &&
            !TryGetSourceStats(gun.Owner, out side, out stats))
        {
            return;
        }

        var fired = Math.Max(1, args.Ammo.Count);
        if (side == CCMStatsSide.Marines)
            stats.MarineShotsFired += fired;
        else if (side == CCMStatsSide.Xenos)
            stats.XenoShotsFired += fired;
    }

    private void HandleHealingChanged(EntityUid target, CCMStatsSide targetSide, DamageChangedEvent args, int healing)
    {
        if (!TryGetSourceStats(args.Origin, args.Tool, out var sourceSide, out var stats))
            return;

        if (args.Origin == target)
            return;

        if (sourceSide == CCMStatsSide.Marines)
        {
            stats.MarineHealingDone += healing;
            stats.MarineImpact += healing * HealingImpactFactor;
            return;
        }

        if (sourceSide != CCMStatsSide.Xenos)
            return;

        stats.XenoHealingDone += healing;
        stats.XenoImpact += healing * HealingImpactFactor;
    }

    private void HandleDamageChanged(EntityUid target, CCMStatsSide targetSide, DamageChangedEvent args, float damage)
    {
        if (!TryGetSourceStats(args.Origin, args.Tool, out var sourceSide, out var stats))
        {
            if (targetSide != CCMStatsSide.None)
                LogUnattributedCombatDamage(target, args, targetSide);
            else
                EnqueueDamageDiagnostic(
                    damage,
                    CCMStatsSide.None,
                    "unknown-target-unresolved-source",
                    ToPrettyString(target),
                    ToPrettyString(args.Origin),
                    ToPrettyString(args.Tool));
            return;
        }

        if (args.Origin == target)
            return;

        if (targetSide != CCMStatsSide.None)
        {
            var damageEvent = new CCMCombatDamageRecordedEvent(
                stats.Player,
                sourceSide,
                targetSide,
                damage,
                sourceSide == targetSide);
            RaiseLocalEvent(damageEvent);
        }

        if (targetSide == CCMStatsSide.None)
        {
            if (sourceSide == CCMStatsSide.Marines)
            {
                _fallbackMarineDamage += damage;
                _fallbackMarineHits += 1;
            }
            else if (sourceSide == CCMStatsSide.Xenos)
            {
                _fallbackXenoDamage += damage;
                _fallbackXenoHits += 1;
            }

            EnqueueDamageDiagnostic(
                damage,
                sourceSide,
                "unknown-target-attributed-source",
                ToPrettyString(target),
                ToPrettyString(args.Origin),
                ToPrettyString(args.Tool));
        }

        if (sourceSide == CCMStatsSide.Marines)
        {
            stats.MarineDamage += damage;
            stats.MarineImpact += damage * DamageImpactFactor;
            return;
        }

        if (sourceSide != CCMStatsSide.Xenos)
            return;

        stats.XenoDamage += damage;
        stats.XenoImpact += damage * DamageImpactFactor;
    }

    private void OnKillReported(ref KillReportedEvent args)
    {
        var victimSide = GetSide(args.Entity);
        if (victimSide == CCMStatsSide.Xenos)
        {
            if (TryResolvePlayerAndSide(args.Entity, out var victimUserId, out _))
                GetOrCreateRoundStats(victimUserId).XenoDeaths += 1;
        }
        else if (victimSide == CCMStatsSide.Marines)
        {
            if (TryResolvePlayerAndSide(args.Entity, out var victimUserId, out _))
                GetOrCreateRoundStats(victimUserId).MarineDeaths += 1;
        }

        if (args.Primary is not KillPlayerSource player || args.Suicide)
            return;

        var killerSide = player.Side != CCMStatsSide.None
            ? player.Side
            : GetPlayerCurrentSide(player.PlayerId);
        if (killerSide == CCMStatsSide.None)
            return;

        if (victimSide == CCMStatsSide.Xenos)
        {
            if (killerSide != CCMStatsSide.Marines)
                return;

            var stats = GetOrCreateRoundStats(player.PlayerId);
            stats.MarineKills += 1;
            stats.MarineImpact += MarineKillImpactPoints;
        }
        else if (victimSide == CCMStatsSide.Marines)
        {
            if (killerSide != CCMStatsSide.Xenos)
                return;

            var stats = GetOrCreateRoundStats(player.PlayerId);
            stats.XenoKills += 1;
            stats.XenoImpact += XenoKillImpactPoints;
        }
    }

    private void OnProjectileShot(Entity<ProjectileComponent> ent, ref ProjectileShotEvent args)
    {
        if (args.Shooter is { } shotOwner)
            TryStampProjectileSource(ent.Owner, shotOwner);

        if (args.Shooter is not { } shooter)
            return;

        var side = GetSide(shooter);
        if (side != CCMStatsSide.Xenos || !TryGetEntityStats(shooter, side, out var stats))
            return;

        stats.XenoShotsFired += 1;
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.OldMobState != MobState.Critical ||
            args.NewMobState != MobState.Alive ||
            GetSide(args.Target) != CCMStatsSide.Marines)
        {
            return;
        }

        if (_countedDefibRevives.Remove(args.Target))
            return;

        if (!TryGetSourceStats(args.Origin, null, CCMStatsSide.Marines, out var stats))
            return;

        stats.MarineRevives += 1;
    }

    private void OnTargetDefibrillated(ref TargetDefibrillatedEvent args)
    {
        if (!args.RevivedFromDeath || GetSide(args.Target) != CCMStatsSide.Marines)
            return;

        if (!TryGetEntityStats(args.User, CCMStatsSide.Marines, out var stats))
            return;

        stats.MarineRevives += 1;
        _countedDefibRevives.Add(args.Target);
    }

    private void OnMarineStructureBuilt(RMCStructureBuiltEvent args)
    {
        if (!TryGetEntityStats(args.User, CCMStatsSide.Marines, out var stats))
            return;

        AwardMarineStructures(stats, Math.Max(1, args.Count));
    }

    private void OnXenoStructureBuilt(XenoStructureBuiltEvent args)
    {
        if (!TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        AwardXenoStructures(stats, 1);
    }

    private void OnXenoStructureUpgraded(XenoStructureUpgradedEvent args)
    {
        if (!TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        AwardXenoStructures(stats, 1);
    }

    private void OnXenoResinHolePlaced(XenoResinHolePlacedEvent args)
    {
        if (!TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        AwardXenoStructures(stats, 1);
    }

    private void OnXenoTunnelPlaced(XenoTunnelPlacedEvent args)
    {
        if (!TryGetEntityStats(args.User, CCMStatsSide.Xenos, out var stats))
            return;

        AwardXenoStructures(stats, 1);
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
            var marineDamage = Math.Max(0, (int) MathF.Round(stats.MarineDamage) - stats.PersistedMarineDamage);
            var xenoDamage = Math.Max(0, (int) MathF.Round(stats.XenoDamage) - stats.PersistedXenoDamage);
            var marineDeaths = Math.Max(0, stats.MarineDeaths - stats.PersistedMarineDeaths);
            var xenoDeaths = Math.Max(0, stats.XenoDeaths - stats.PersistedXenoDeaths);
            var marineShots = Math.Max(0, stats.MarineShotsFired - stats.PersistedMarineShotsFired);
            var xenoShots = Math.Max(0, stats.XenoShotsFired - stats.PersistedXenoShotsFired);
            var marineImpact = Math.Max(0, (int) MathF.Round(stats.MarineImpact) - stats.PersistedMarineImpactPoints);
            var xenoImpact = Math.Max(0, (int) MathF.Round(stats.XenoImpact) - stats.PersistedXenoImpactPoints);
            var totalKills = marineKills + xenoKills;
            var totalRevives = marineRevives;
            var totalHealingDone = marineHealingDone + xenoHealingDone;
            var totalStructuresBuilt = marineStructuresBuilt + xenoStructuresBuilt;
            var totalDamage = marineDamage + xenoDamage;
            var totalDeaths = marineDeaths + xenoDeaths;
            var totalShots = marineShots + xenoShots;
            var totalImpact = marineImpact + xenoImpact;

            saveTasks.Add(_db.SaveCCMRoundStats(
                player,
                now.Year,
                now.Month,
                stats.GeneralRoundsPlayed,
                stats.GeneralRoundsWon,
                stats.GeneralRoundsLost,
                (int) stats.RoundSecondsPlayed,
                totalDamage,
                totalKills,
                stats.VictoryPointsEarned,
                totalImpact,
                totalRevives,
                totalHealingDone,
                totalStructuresBuilt,
                totalDeaths,
                totalShots,
                stats.MarineRoundsPlayed,
                stats.MarineRoundsWon,
                stats.MarineRoundsLost,
                marineDamage,
                marineKills,
                stats.MarineVictoryPointsEarned,
                marineImpact,
                marineRevives,
                marineHealingDone,
                marineStructuresBuilt,
                marineDeaths,
                marineShots,
                stats.XenoRoundsPlayed,
                stats.XenoRoundsWon,
                stats.XenoRoundsLost,
                xenoDamage,
                xenoKills,
                stats.XenoVictoryPointsEarned,
                xenoImpact,
                xenoHealingDone,
                xenoStructuresBuilt,
                xenoDeaths,
                xenoShots));

            stats.PersistedMarineKills += marineKills;
            stats.PersistedXenoKills += xenoKills;
            stats.PersistedMarineRevives += marineRevives;
            stats.PersistedMarineHealingDone += marineHealingDone;
            stats.PersistedXenoHealingDone += xenoHealingDone;
            stats.PersistedMarineStructuresBuilt += marineStructuresBuilt;
            stats.PersistedXenoStructuresBuilt += xenoStructuresBuilt;
            stats.PersistedMarineDamage += marineDamage;
            stats.PersistedXenoDamage += xenoDamage;
            stats.PersistedMarineDeaths += marineDeaths;
            stats.PersistedXenoDeaths += xenoDeaths;
            stats.PersistedMarineShotsFired += marineShots;
            stats.PersistedXenoShotsFired += xenoShots;
            stats.PersistedMarineImpactPoints += marineImpact;
            stats.PersistedXenoImpactPoints += xenoImpact;
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
                var marineDamage = Math.Max(0, (int) MathF.Round(stats.MarineDamage) - stats.PersistedMarineDamage);
                var xenoDamage = Math.Max(0, (int) MathF.Round(stats.XenoDamage) - stats.PersistedXenoDamage);
                var marineDeaths = Math.Max(0, stats.MarineDeaths - stats.PersistedMarineDeaths);
                var xenoDeaths = Math.Max(0, stats.XenoDeaths - stats.PersistedXenoDeaths);
                var marineShots = Math.Max(0, stats.MarineShotsFired - stats.PersistedMarineShotsFired);
                var xenoShots = Math.Max(0, stats.XenoShotsFired - stats.PersistedXenoShotsFired);
                var marineImpact = Math.Max(0, (int) MathF.Round(stats.MarineImpact) - stats.PersistedMarineImpactPoints);
                var xenoImpact = Math.Max(0, (int) MathF.Round(stats.XenoImpact) - stats.PersistedXenoImpactPoints);

                var hasProgress = marineKills > 0 ||
                                  xenoKills > 0 ||
                                  marineRevives > 0 ||
                                  marineHealingDone > 0 ||
                                  xenoHealingDone > 0 ||
                                  marineStructuresBuilt > 0 ||
                                  xenoStructuresBuilt > 0 ||
                                  marineDamage > 0 ||
                                  xenoDamage > 0 ||
                                  marineDeaths > 0 ||
                                  xenoDeaths > 0 ||
                                  marineShots > 0 ||
                                  xenoShots > 0 ||
                                  marineImpact > 0 ||
                                  xenoImpact > 0;

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
                    marineDamage + xenoDamage,
                    marineKills + xenoKills,
                    0,
                    marineImpact + xenoImpact,
                    marineRevives,
                    marineHealingDone + xenoHealingDone,
                    marineStructuresBuilt + xenoStructuresBuilt,
                    marineDeaths + xenoDeaths,
                    marineShots + xenoShots,
                    0,
                    0,
                    0,
                    marineDamage,
                    marineKills,
                    0,
                    marineImpact,
                    marineRevives,
                    marineHealingDone,
                    marineStructuresBuilt,
                    marineDeaths,
                    marineShots,
                    0,
                    0,
                    0,
                    xenoDamage,
                    xenoKills,
                    0,
                    xenoImpact,
                    xenoHealingDone,
                    xenoStructuresBuilt,
                    xenoDeaths,
                    xenoShots));

                stats.PersistedMarineKills += marineKills;
                stats.PersistedXenoKills += xenoKills;
                stats.PersistedMarineRevives += marineRevives;
                stats.PersistedMarineHealingDone += marineHealingDone;
                stats.PersistedXenoHealingDone += xenoHealingDone;
                stats.PersistedMarineStructuresBuilt += marineStructuresBuilt;
                stats.PersistedXenoStructuresBuilt += xenoStructuresBuilt;
                stats.PersistedMarineDamage += marineDamage;
                stats.PersistedXenoDamage += xenoDamage;
                stats.PersistedMarineDeaths += marineDeaths;
                stats.PersistedXenoDeaths += xenoDeaths;
                stats.PersistedMarineShotsFired += marineShots;
                stats.PersistedXenoShotsFired += xenoShots;
                stats.PersistedMarineImpactPoints += marineImpact;
                stats.PersistedXenoImpactPoints += xenoImpact;
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
                    _ticker.RoundId,
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
            : best.Value.LastKnownCkey ?? best.Key.ToString();
        var name = TryGetCurrentName(best.Key, out var netEntity, out var resolvedName)
            ? resolvedName
            : best.Value.LastKnownName ?? ckey;

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

    private void UpdateLastKnownIdentity(NetUserId userId, EntityUid? entity = null)
    {
        var stats = GetOrCreateRoundStats(userId);

        if (_players.TryGetSessionById(userId, out var session))
        {
            if (!string.IsNullOrWhiteSpace(session.Name))
                stats.LastKnownCkey = session.Name;

            entity ??= session.AttachedEntity;
        }

        if (entity is not { } resolvedEntity)
            return;

        var entityName = MetaData(resolvedEntity).EntityName;
        if (!string.IsNullOrWhiteSpace(entityName))
            stats.LastKnownName = entityName;
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
        UpdateLastKnownIdentity(player);
        if (side != CCMStatsSide.None)
            stats.LastKnownSide = side;

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

    private bool IsRoundStatsTrackingActive()
    {
        return !_roundFinalized && _ticker.RunLevel != GameRunLevel.PostRound;
    }

    private void StartActiveParticipation(NetUserId player, EntityUid entity)
    {
        UpdateLastKnownIdentity(player, entity);
        var stats = GetOrCreateRoundStats(player);
        StopActiveParticipation(stats);

        var side = GetSide(entity);
        if (side == CCMStatsSide.None)
            return;

        stats.LastKnownSide = side;
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
        if (HasComp<RMCSurvivorComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<SynthComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<XenoComponent>(uid))
            return CCMStatsSide.Xenos;
        return CCMStatsSide.None;
    }

    private void EnsureStatsKillTracker(EntityUid uid)
    {
        _killTracking.EnsureKillTracker(uid, MobState.Dead);
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

        if (_roundStats.TryGetValue(player, out var stats))
        {
            if (stats.ActiveSide != CCMStatsSide.None)
                return stats.ActiveSide;

            return stats.LastKnownSide;
        }

        return CCMStatsSide.None;
    }

    private CCMRoundPersonalStatsData BuildPersonalStats(RoundPlayerStats stats)
    {
        var marineDamage = (int) MathF.Round(stats.MarineDamage);
        var xenoDamage = (int) MathF.Round(stats.XenoDamage);
        var marineImpact = (int) MathF.Round(stats.MarineImpact);
        var xenoImpact = (int) MathF.Round(stats.XenoImpact);
        var totalKills = stats.MarineKills + stats.XenoKills;
        var totalHealing = stats.MarineHealingDone + stats.XenoHealingDone;
        var totalStructures = stats.MarineStructuresBuilt + stats.XenoStructuresBuilt;
        var totalDamage = marineDamage + xenoDamage;
        var totalImpact = marineImpact + xenoImpact;
        var victoryPoints = stats.MarineVictoryPointsEarned + stats.XenoVictoryPointsEarned;

        return new CCMRoundPersonalStatsData(
            victoryPoints + totalKills,
            victoryPoints,
            totalImpact,
            totalDamage,
            totalKills,
            totalHealing,
            stats.MarineRevives,
            totalStructures,
            (int) Math.Round(stats.RoundSecondsPlayed),
            stats.MarineParticipated,
            stats.XenoParticipated,
            stats.MarineVictoryPointsEarned,
            marineImpact,
            marineDamage,
            stats.MarineKills,
            stats.MarineHealingDone,
            stats.MarineRevives,
            stats.MarineStructuresBuilt,
            stats.XenoVictoryPointsEarned,
            xenoImpact,
            xenoDamage,
            stats.XenoKills,
            stats.XenoHealingDone,
            stats.XenoStructuresBuilt);
    }

    private bool TryGetSourceStats(EntityUid? origin, EntityUid? tool, CCMStatsSide expectedSide, out RoundPlayerStats stats)
    {
        if (TryGetSourceStats(origin, out var side, out stats) && side == expectedSide)
            return true;

        if (TryGetSourceStats(tool, out side, out stats) && side == expectedSide)
            return true;

        stats = default!;
        return false;
    }

    private bool TryGetSourceStats(EntityUid? origin, EntityUid? tool, out CCMStatsSide side, out RoundPlayerStats stats)
    {
        if (TryGetSourceStats(origin, out side, out stats))
            return true;

        return TryGetSourceStats(tool, out side, out stats);
    }

    private bool TryGetSourceStats(EntityUid? source, out CCMStatsSide side, out RoundPlayerStats stats)
    {
        side = CCMStatsSide.None;
        stats = default!;

        if (source == null || !TryResolvePlayerAndSide(source.Value, out var userId, out side))
            return false;

        if (side == CCMStatsSide.None)
            return false;

        stats = GetOrCreateRoundStats(userId);
        return true;
    }

    private bool TryGetEntityStats(EntityUid? entity, CCMStatsSide expectedSide, out RoundPlayerStats stats)
    {
        stats = default!;

        if (entity == null)
            return false;

        if (TryComp(entity.Value, out CCMStatsProjectileSourceComponent? projectileSource))
        {
            if (projectileSource.Side != expectedSide)
                return false;

            stats = GetOrCreateRoundStats(projectileSource.UserId);
            return true;
        }

        if (!TryResolvePlayerAndSide(entity.Value, out var userId, out var side))
            return false;

        if (side != expectedSide)
            return false;

        stats = GetOrCreateRoundStats(userId);
        return true;
    }

    private bool TryResolvePlayerAndSide(EntityUid entity, out NetUserId userId, out CCMStatsSide side)
    {
        userId = default;
        side = CCMStatsSide.None;

        var visited = new HashSet<EntityUid>();
        return TryResolvePlayerAndSide(entity, visited, ref userId, ref side);
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

            if (TryComp(current, out ActionComponent? action))
            {
                if (action.AttachedEntity is { } attached &&
                    attached != current &&
                    TryResolvePlayerAndSide(attached, visited, ref userId, ref side))
                {
                    return true;
                }

                if (action.Container is { } container &&
                    container != current &&
                    TryResolvePlayerAndSide(container, visited, ref userId, ref side))
                {
                    return true;
                }
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

    private void TryStampProjectileSource(EntityUid projectile, EntityUid shooter)
    {
        if (!TryResolvePlayerAndSide(shooter, out var userId, out var side) ||
            side == CCMStatsSide.None)
        {
            return;
        }

        var source = EnsureComp<CCMStatsProjectileSourceComponent>(projectile);
        source.UserId = userId;
        source.Side = side;
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
        return total < 0 ? (int) MathF.Round(-total) : 0;
    }

    private void LogUnattributedCombatDamage(EntityUid target, DamageChangedEvent args, CCMStatsSide targetSide)
    {
        var damage = GetPositiveDamage(args);
        if (damage <= 0)
            return;

        switch (targetSide)
        {
            case CCMStatsSide.Marines:
                _unattributedDamageToMarines += damage;
                _unattributedHitsToMarines += 1;
                break;
            case CCMStatsSide.Xenos:
                _unattributedDamageToXenos += damage;
                _unattributedHitsToXenos += 1;
                break;
        }

        EnqueueDamageDiagnostic(
            damage,
            targetSide,
            "unattributed-source",
            ToPrettyString(target),
            ToPrettyString(args.Origin),
            ToPrettyString(args.Tool));

        Log.Debug(
            $"CCM stats could not attribute {damage:0.##} damage to {targetSide} target {ToPrettyString(target)}. " +
            $"origin={ToPrettyString(args.Origin)} tool={ToPrettyString(args.Tool)}");
    }

    private void HandleUnknownTargetDamage(EntityUid target, DamageChangedEvent args, float damage)
    {
        if (!TryGetSourceStats(args.Origin, args.Tool, out var sourceSide, out var stats) ||
            sourceSide == CCMStatsSide.None ||
            args.Origin == target)
        {
            EnqueueDamageDiagnostic(
                damage,
                CCMStatsSide.None,
                "unknown-target-unresolved-source",
                ToPrettyString(target),
                ToPrettyString(args.Origin),
                ToPrettyString(args.Tool));
            return;
        }

        // If the victim chain no longer resolves to a marine/xeno body, still preserve
        // the dealt-damage counter so round-end doesn't silently drop the hit.
        if (sourceSide == CCMStatsSide.Marines)
        {
            stats.MarineDamage += damage;
            _fallbackMarineDamage += damage;
            _fallbackMarineHits += 1;
        }
        else if (sourceSide == CCMStatsSide.Xenos)
        {
            stats.XenoDamage += damage;
            _fallbackXenoDamage += damage;
            _fallbackXenoHits += 1;
        }

        EnqueueDamageDiagnostic(
            damage,
            sourceSide,
            "fallback-unknown-target",
            ToPrettyString(target),
            ToPrettyString(args.Origin),
            ToPrettyString(args.Tool));

        Log.Debug(
            $"CCM stats fallback-attributed {damage:0.##} damage for {sourceSide} source against unresolved target {ToPrettyString(target)}. " +
            $"origin={ToPrettyString(args.Origin)} tool={ToPrettyString(args.Tool)}");
    }

    private void EnqueueDamageDiagnostic(
        float damage,
        CCMStatsSide side,
        string reason,
        string? target,
        string? origin,
        string? tool)
    {
        if (_recentDamageDiagnostics.Count >= DamageDiagnosticsHistoryLimit)
            _recentDamageDiagnostics.Dequeue();

        _recentDamageDiagnostics.Enqueue(
            new DamageDiagnosticEntry(
                _timing.CurTime,
                damage,
                side,
                reason,
                target ?? "<null>",
                origin ?? "<null>",
                tool ?? "<null>"));
    }

    public CCMRoundDamageDebugSnapshot GetDamageDebugSnapshot(int maxPlayers = 25)
    {
        if (maxPlayers < 1)
            maxPlayers = 1;

        var players = _roundStats.Values
            .Where(stats => stats.MarineDamage > 0 || stats.XenoDamage > 0 || stats.MarineKills > 0 || stats.XenoKills > 0)
            .OrderByDescending(stats => stats.MarineDamage + stats.XenoDamage)
            .ThenBy(stats => stats.LastKnownCkey ?? stats.Player.ToString())
            .Take(maxPlayers)
            .Select(stats => new CCMRoundDamagePlayerDebugSnapshot(
                stats.Player.ToString(),
                stats.LastKnownCkey ?? stats.Player.ToString(),
                stats.LastKnownName ?? stats.LastKnownCkey ?? stats.Player.ToString(),
                stats.LastKnownSide,
                (int) MathF.Round(stats.MarineDamage),
                stats.MarineHealingDone,
                (int) MathF.Round(stats.XenoDamage),
                stats.XenoHealingDone,
                stats.MarineKills,
                stats.XenoKills,
                stats.MarineParticipated,
                stats.XenoParticipated))
            .ToArray();

        return new CCMRoundDamageDebugSnapshot(
            players,
            _recentDamageDiagnostics.ToArray(),
            (int) MathF.Round(_roundStats.Values.Sum(stats => stats.MarineDamage)),
            _roundStats.Values.Sum(stats => stats.MarineHealingDone),
            (int) MathF.Round(_roundStats.Values.Sum(stats => stats.XenoDamage)),
            _roundStats.Values.Sum(stats => stats.XenoHealingDone),
            (int) MathF.Round(_unattributedDamageToMarines),
            (int) MathF.Round(_unattributedDamageToXenos),
            _unattributedHitsToMarines,
            _unattributedHitsToXenos,
            (int) MathF.Round(_fallbackMarineDamage),
            (int) MathF.Round(_fallbackXenoDamage),
            _fallbackMarineHits,
            _fallbackXenoHits);
    }

    private void AwardMarineStructures(RoundPlayerStats stats, int count)
    {
        if (count <= 0)
            return;

        stats.MarineStructuresBuilt += count;
        stats.MarineImpact += MarineStructureImpactPoints * count;
    }

    private void AwardXenoStructures(RoundPlayerStats stats, int count)
    {
        if (count <= 0)
            return;

        stats.XenoStructuresBuilt += count;
        stats.XenoImpact += XenoStructureImpactPoints * count;
    }

    private CCMPlayerStatsSnapshot MergeLiveSnapshot(CCMPlayerStatsSnapshot snapshot, RoundPlayerStats live)
    {
        var includeActiveRound = !_roundFinalized && live.HadAnyParticipation;
        var activeSeconds = live.ActiveSide != CCMStatsSide.None && live.ActiveSince != null
            ? Math.Max(0, (_timing.CurTime - live.ActiveSince.Value).TotalSeconds)
            : 0;
        var roundSeconds = (int) Math.Round(live.RoundSecondsPlayed + activeSeconds);

        var marineDamage = Math.Max(0, (int) MathF.Round(live.MarineDamage) - live.PersistedMarineDamage);
        var xenoDamage = Math.Max(0, (int) MathF.Round(live.XenoDamage) - live.PersistedXenoDamage);
        var marineKills = Math.Max(0, live.MarineKills - live.PersistedMarineKills);
        var xenoKills = Math.Max(0, live.XenoKills - live.PersistedXenoKills);
        var marineRevives = Math.Max(0, live.MarineRevives - live.PersistedMarineRevives);
        var marineHealingDone = Math.Max(0, live.MarineHealingDone - live.PersistedMarineHealingDone);
        var xenoHealingDone = Math.Max(0, live.XenoHealingDone - live.PersistedXenoHealingDone);
        var marineStructuresBuilt = Math.Max(0, live.MarineStructuresBuilt - live.PersistedMarineStructuresBuilt);
        var xenoStructuresBuilt = Math.Max(0, live.XenoStructuresBuilt - live.PersistedXenoStructuresBuilt);
        var marineDeaths = Math.Max(0, live.MarineDeaths - live.PersistedMarineDeaths);
        var xenoDeaths = Math.Max(0, live.XenoDeaths - live.PersistedXenoDeaths);
        var marineShotsFired = Math.Max(0, live.MarineShotsFired - live.PersistedMarineShotsFired);
        var xenoShotsFired = Math.Max(0, live.XenoShotsFired - live.PersistedXenoShotsFired);
        var marineImpactPoints = Math.Max(0, (int) MathF.Round(live.MarineImpact) - live.PersistedMarineImpactPoints);
        var xenoImpactPoints = Math.Max(0, (int) MathF.Round(live.XenoImpact) - live.PersistedXenoImpactPoints);

        return new CCMPlayerStatsSnapshot(
            snapshot.RoundsPlayed + (includeActiveRound ? 1 : 0),
            snapshot.RoundsWon,
            snapshot.RoundsLost,
            snapshot.RoundSecondsPlayed + roundSeconds,
            snapshot.TotalDamageDealt + marineDamage + xenoDamage,
            snapshot.TotalKills + marineKills + xenoKills,
            snapshot.VictoryPoints,
            snapshot.ImpactPoints + marineImpactPoints + xenoImpactPoints,
            snapshot.Revives + marineRevives,
            snapshot.HealingDone + marineHealingDone + xenoHealingDone,
            snapshot.StructuresBuilt + marineStructuresBuilt + xenoStructuresBuilt,
            snapshot.Deaths + marineDeaths + xenoDeaths,
            snapshot.ShotsFired + marineShotsFired + xenoShotsFired,
            snapshot.MarineRoundsPlayed + (includeActiveRound && live.MarineParticipated ? 1 : 0),
            snapshot.MarineRoundsWon,
            snapshot.MarineRoundsLost,
            snapshot.MarineDamageDealt + marineDamage,
            snapshot.MarineKills + marineKills,
            snapshot.MarineVictoryPoints,
            snapshot.MarineImpactPoints + marineImpactPoints,
            snapshot.MarineRevives + marineRevives,
            snapshot.MarineHealingDone + marineHealingDone,
            snapshot.MarineStructuresBuilt + marineStructuresBuilt,
            snapshot.MarineDeaths + marineDeaths,
            snapshot.MarineShotsFired + marineShotsFired,
            snapshot.XenoRoundsPlayed + (includeActiveRound && live.XenoParticipated ? 1 : 0),
            snapshot.XenoRoundsWon,
            snapshot.XenoRoundsLost,
            snapshot.XenoDamageDealt + xenoDamage,
            snapshot.XenoKills + xenoKills,
            snapshot.XenoVictoryPoints,
            snapshot.XenoImpactPoints + xenoImpactPoints,
            snapshot.XenoHealingDone + xenoHealingDone,
            snapshot.XenoStructuresBuilt + xenoStructuresBuilt,
            snapshot.XenoDeaths + xenoDeaths,
            snapshot.XenoShotsFired + xenoShotsFired);
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
        public string? LastKnownName;
        public string? LastKnownCkey;
        public CCMStatsSide LastKnownSide = CCMStatsSide.None;

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
        public int PersistedMarineDamage;
        public int PersistedXenoDamage;
        public int PersistedMarineDeaths;
        public int PersistedXenoDeaths;
        public int PersistedMarineShotsFired;
        public int PersistedXenoShotsFired;
        public int PersistedMarineImpactPoints;
        public int PersistedXenoImpactPoints;
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

    public readonly record struct CCMRoundDamagePlayerDebugSnapshot(
        string UserId,
        string Ckey,
        string Name,
        CCMStatsSide LastKnownSide,
        int MarineDamage,
        int MarineHealingDone,
        int XenoDamage,
        int XenoHealingDone,
        int MarineKills,
        int XenoKills,
        bool MarineParticipated,
        bool XenoParticipated);

    public readonly record struct DamageDiagnosticEntry(
        TimeSpan Time,
        float Damage,
        CCMStatsSide Side,
        string Reason,
        string Target,
        string Origin,
        string Tool);

    public readonly record struct CCMRoundDamageDebugSnapshot(
        CCMRoundDamagePlayerDebugSnapshot[] Players,
        DamageDiagnosticEntry[] RecentDiagnostics,
        int TotalMarineDamage,
        int TotalMarineHealing,
        int TotalXenoDamage,
        int TotalXenoHealing,
        int UnattributedDamageToMarines,
        int UnattributedDamageToXenos,
        int UnattributedHitsToMarines,
        int UnattributedHitsToXenos,
        int FallbackMarineDamage,
        int FallbackXenoDamage,
        int FallbackMarineHits,
        int FallbackXenoHits);

    public readonly record struct CCMCombatDamageRecordedEvent(
        NetUserId UserId,
        CCMStatsSide SourceSide,
        CCMStatsSide TargetSide,
        float Damage,
        bool FriendlyFire);
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
