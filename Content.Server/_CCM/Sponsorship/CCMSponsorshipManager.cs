// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.Secrets.CCM.Sponsorship;
using Content.Server.Station.Systems;
using Content.Shared._CCM.Sponsorship;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Sponsorship;

public sealed class CCMSponsorshipManager : IPostInjectInit
{
    private static readonly TimeSpan DefaultManualSponsorshipDuration = TimeSpan.FromDays(30);
    private readonly Dictionary<NetUserId, CCMSponsorshipStatusSnapshot> _baseStatus = new();
    private readonly Dictionary<NetUserId, CCMSponsorshipStatusSnapshot> _status = new();
    private readonly Dictionary<NetUserId, CCMStoredSponsorshipRecord> _manualTierOverrides = new();

    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ICCMSponsorshipSecretsProvider _provider = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private StationJobsSystem StationJobs => _entManager.System<StationJobsSystem>();

    public CCMSponsorshipStatusSnapshot GetStatus(NetUserId userId)
    {
        return _status.GetValueOrDefault(userId) ?? new CCMSponsorshipStatusSnapshot(
            CCMSponsorshipTier.None,
            _provider.DonateUrl,
            0,
            string.Empty,
            string.Empty,
            0f,
            false,
            false);
    }

    public bool HasRoleTimerBypass(NetUserId userId)
    {
        return GetStatus(userId).Tier >= CCMSponsorshipTier.SponsorII;
    }

    public bool HasRoleTimerBypass(ICommonSession session)
    {
        return HasRoleTimerBypass(session.UserId);
    }

    public void SetManualTierOverride(NetUserId userId, CCMSponsorshipTier tier, long? expirationUnixSeconds = null)
    {
        if (tier == CCMSponsorshipTier.None)
            _manualTierOverrides.Remove(userId);
        else
            _manualTierOverrides[userId] = new CCMStoredSponsorshipRecord(
                tier,
                expirationUnixSeconds ?? ResolveExpiration(tier, 0));

        RefreshResolvedStatus(userId);
    }

    public Task PersistTier(NetUserId userId, CCMSponsorshipTier tier, int days)
    {
        var expiration = tier == CCMSponsorshipTier.None
            ? 0
            : DateTimeOffset.UtcNow.AddDays(Math.Max(1, days)).ToUnixTimeSeconds();

        return _db.SaveCCMStoredSponsorship(userId.UserId, tier, expiration);
    }

    public async Task SetPersistentTier(NetUserId userId, CCMSponsorshipTier tier, int days)
    {
        var expiration = tier == CCMSponsorshipTier.None
            ? 0
            : DateTimeOffset.UtcNow.AddDays(Math.Max(1, days)).ToUnixTimeSeconds();

        await _db.SaveCCMStoredSponsorship(userId.UserId, tier, expiration);
        SetManualTierOverride(userId, tier, expiration);
    }

    public bool TryGetChatColorHex(NetUserId userId, bool looc, out string colorHex)
    {
        var status = GetStatus(userId);
        colorHex = looc ? status.LoocColorHex : status.OocColorHex;
        return !string.IsNullOrWhiteSpace(colorHex);
    }

    public IReadOnlyList<(string Ckey, CCMSponsorshipTier Tier)> GetConnectedSponsorsForCredits()
    {
        return _players.Sessions
            .Select(session => (session.Name, GetStatus(session.UserId).Tier))
            .Where(entry => entry.Tier != CCMSponsorshipTier.None)
            .OrderByDescending(entry => entry.Tier)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Select(entry => (entry.Name, entry.Tier))
            .ToList();
    }

    public async Task<bool> RefreshSession(ICommonSession session, CancellationToken cancel)
    {
        var before = GetStatus(session.UserId);
        await LoadData(session, cancel);
        var after = GetStatus(session.UserId);
        return !StatusEquals(before, after);
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var record = await _provider.GetStatusAsync(session.UserId.UserId, session.Name, cancel);
        var stored = await _db.GetCCMStoredSponsorship(session.UserId.UserId);
        cancel.ThrowIfCancellationRequested();

        var providerSnapshot = new CCMSponsorshipStatusSnapshot(
            record.Tier,
            _provider.DonateUrl,
            ResolveExpiration(record.Tier, record.ExpirationUnixSeconds),
            string.IsNullOrWhiteSpace(record.OocColorHex) ? GetDefaultColor(record.Tier, false) : record.OocColorHex,
            string.IsNullOrWhiteSpace(record.LoocColorHex) ? GetDefaultColor(record.Tier, true) : record.LoocColorHex,
            GetRoleWeightBonus(record.Tier),
            record.QueueBypass,
            record.Tier != CCMSponsorshipTier.None);

        var snapshot = ApplyStoredSponsorship(providerSnapshot, stored);
        _baseStatus[session.UserId] = snapshot;
        RefreshResolvedStatus(session.UserId);
    }

    private void ClientDisconnected(ICommonSession session)
    {
        _baseStatus.Remove(session.UserId);
        _status.Remove(session.UserId);
        _manualTierOverrides.Remove(session.UserId);
        StationJobs.ClearExternalWeightOverride(session.UserId);
    }

    private void RefreshResolvedStatus(NetUserId userId)
    {
        var baseSnapshot = _baseStatus.GetValueOrDefault(userId) ?? new CCMSponsorshipStatusSnapshot(
            CCMSponsorshipTier.None,
            _provider.DonateUrl,
            0,
            string.Empty,
            string.Empty,
            0f,
            false,
            false);

        var resolved = ApplyManualOverride(userId, baseSnapshot);
        _status[userId] = resolved;
        ApplyRoleWeightOverride(userId, resolved);
    }

    private CCMSponsorshipStatusSnapshot ApplyStoredSponsorship(
        CCMSponsorshipStatusSnapshot fallback,
        CCMStoredSponsorshipRecord? stored)
    {
        if (stored == null || stored.Tier == CCMSponsorshipTier.None)
            return fallback;

        if (stored.ExpirationUnixSeconds <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return fallback;

        return new CCMSponsorshipStatusSnapshot(
            stored.Tier,
            fallback.DonateUrl,
            stored.ExpirationUnixSeconds,
            GetDefaultColor(stored.Tier, false),
            GetDefaultColor(stored.Tier, true),
            GetRoleWeightBonus(stored.Tier),
            stored.Tier != CCMSponsorshipTier.None,
            stored.Tier != CCMSponsorshipTier.None);
    }

    private CCMSponsorshipStatusSnapshot ApplyManualOverride(NetUserId userId, CCMSponsorshipStatusSnapshot snapshot)
    {
        if (!_manualTierOverrides.TryGetValue(userId, out var overrideRecord))
            return snapshot;

        return new CCMSponsorshipStatusSnapshot(
            overrideRecord.Tier,
            snapshot.DonateUrl,
            ResolveExpiration(overrideRecord.Tier, overrideRecord.ExpirationUnixSeconds),
            GetDefaultColor(overrideRecord.Tier, false),
            GetDefaultColor(overrideRecord.Tier, true),
            GetRoleWeightBonus(overrideRecord.Tier),
            overrideRecord.Tier != CCMSponsorshipTier.None,
            overrideRecord.Tier != CCMSponsorshipTier.None);
    }

    private void ApplyRoleWeightOverride(NetUserId userId, CCMSponsorshipStatusSnapshot snapshot)
    {
        if (snapshot.RoleWeightBonus <= 0f)
        {
            StationJobs.ClearExternalWeightOverride(userId);
            return;
        }

        StationJobs.SetExternalWeightOverride(userId, snapshot.RoleWeightBonus);
    }

    private static float GetRoleWeightBonus(CCMSponsorshipTier tier)
    {
        return 0f;
    }

    private static string GetDefaultColor(CCMSponsorshipTier tier, bool looc)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorI => looc ? "#7FD7FF" : "#61C9FF",
            CCMSponsorshipTier.SponsorII => looc ? "#F2A7FF" : "#D96CFF",
            CCMSponsorshipTier.SponsorIII => looc ? "#FFE082" : "#F6C453",
            _ => string.Empty,
        };
    }

    private static long ResolveExpiration(CCMSponsorshipTier tier, long expirationUnixSeconds)
    {
        if (tier == CCMSponsorshipTier.None)
            return 0;

        if (expirationUnixSeconds > 0)
            return expirationUnixSeconds;

        return DateTimeOffset.UtcNow.Add(DefaultManualSponsorshipDuration).ToUnixTimeSeconds();
    }

    private static bool StatusEquals(CCMSponsorshipStatusSnapshot left, CCMSponsorshipStatusSnapshot right)
    {
        return left.Tier == right.Tier &&
               left.DonateUrl == right.DonateUrl &&
               left.ExpirationUnixSeconds == right.ExpirationUnixSeconds &&
               left.OocColorHex == right.OocColorHex &&
               left.LoocColorHex == right.LoocColorHex &&
               Math.Abs(left.RoleWeightBonus - right.RoleWeightBonus) < 0.001f &&
               left.QueueBypass == right.QueueBypass &&
               left.CustomizationUnlocked == right.CustomizationUnlocked;
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
