using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._CCM.Database;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CCM.Station.Jobs;

public sealed class JobPriorityWeightManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<(NetUserId UserId, int Slot), Dictionary<ProtoId<JobPrototype>, JobPriorityWeightRecord>> _weights = new();
    private readonly HashSet<NetUserId> _storedUsers = new();

    public IReadOnlyDictionary<ProtoId<JobPrototype>, JobPriorityWeightRecord> GetWeights(NetUserId userId, int slot)
    {
        return _weights.GetValueOrDefault((userId, slot)) ?? new Dictionary<ProtoId<JobPrototype>, JobPriorityWeightRecord>();
    }

    public bool TryGetWeight(NetUserId userId, int slot, ProtoId<JobPrototype> jobId, out JobPriorityWeightRecord? record)
    {
        record = null;
        if (!_weights.TryGetValue((userId, slot), out var byJob))
            return false;

        return byJob.TryGetValue(jobId, out record);
    }

    public JobPriorityWeightRecord GetOrCreateWeight(NetUserId userId, int slot, ProtoId<JobPrototype> jobId)
    {
        if (!_weights.TryGetValue((userId, slot), out var byJob))
        {
            byJob = new Dictionary<ProtoId<JobPrototype>, JobPriorityWeightRecord>();
            _weights[(userId, slot)] = byJob;
        }

        if (!byJob.TryGetValue(jobId, out var record))
        {
            record = new JobPriorityWeightRecord();
            byJob[jobId] = record;
        }

        return record;
    }

    public void ApplyRoundResults(
        NetUserId userId,
        int slot,
        IEnumerable<ProtoId<JobPrototype>> firstOrderJobs,
        ProtoId<JobPrototype>? assignedJob,
        int roundId,
        IReadOnlyDictionary<ProtoId<JobPrototype>, int> roundstartJobSlotCounts)
    {
        if (!_storedUsers.Contains(userId))
            return;

        var updates = new List<JobPriorityWeightUpdate>();
        var assignedFirstOrder = assignedJob != null && firstOrderJobs.Contains(assignedJob.Value);

        foreach (var jobId in firstOrderJobs)
        {
            if (!roundstartJobSlotCounts.TryGetValue(jobId, out var slotCount) || slotCount <= 0)
                continue;

            var record = GetOrCreateWeight(userId, slot, jobId);
            if (assignedFirstOrder && jobId == assignedJob)
            {
                record.MissedRounds = 0;
                record.LastAssignedRoundId = roundId;
            }
            else
            {
                record.MissedRounds++;
            }

            updates.Add(new JobPriorityWeightUpdate(jobId, record.MissedRounds, record.LastAssignedRoundId));
        }

        if (updates.Count == 0)
            return;

        _ = UpdateWeightsAsync(userId.UserId, slot, updates);
    }

    private async Task UpdateWeightsAsync(Guid userId, int slot, IReadOnlyList<JobPriorityWeightUpdate> updates)
    {
        await _db.UpsertJobPriorityWeights(userId, slot, updates);
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        if (!ServerPreferencesManager.ShouldStorePrefs(session.Channel.AuthType))
            return;

        _storedUsers.Add(session.UserId);

        var weights = await _db.GetJobPriorityWeights(session.UserId.UserId, cancel);
        cancel.ThrowIfCancellationRequested();

        foreach (var row in weights)
        {
            var key = (new NetUserId(row.PlayerUserId), row.Slot);
            if (!_weights.TryGetValue(key, out var byJob))
            {
                byJob = new Dictionary<ProtoId<JobPrototype>, JobPriorityWeightRecord>();
                _weights[key] = byJob;
            }

            byJob[new ProtoId<JobPrototype>(row.JobName)] = new JobPriorityWeightRecord
            {
                MissedRounds = row.MissedRounds,
                LastAssignedRoundId = row.LastAssignedRoundId
            };
        }
    }

    private void ClientDisconnected(ICommonSession session)
    {
        var keys = _weights.Keys
            .Where(key => key.UserId == session.UserId)
            .ToList();

        foreach (var key in keys)
        {
            _weights.Remove(key);
        }

        _storedUsers.Remove(session.UserId);
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}

public sealed class JobPriorityWeightRecord
{
    public int MissedRounds;
    public int? LastAssignedRoundId;
}

// # CCM priority rework
