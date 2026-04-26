using System;
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Shared._CCM.Preferences;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Station.Systems;

// Contains code for round-start spawning.
public sealed partial class StationJobsSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IBanManager _banManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    private Dictionary<int, HashSet<string>> _jobsByWeight = default!;
    private List<int> _orderedWeights = default!;

    /// <summary>
    /// Sets up some tables used by AssignJobs, including jobs sorted by their weights, and a list of weights in order from highest to lowest.
    /// </summary>
    private void InitializeRoundStart()
    {
        _jobsByWeight = new Dictionary<int, HashSet<string>>();
        foreach (var job in _prototypeManager.EnumeratePrototypes<JobPrototype>())
        {
            if (!_jobsByWeight.ContainsKey(job.Weight))
                _jobsByWeight.Add(job.Weight, new HashSet<string>());

            _jobsByWeight[job.Weight].Add(job.ID);
        }

        _orderedWeights = _jobsByWeight.Keys.OrderByDescending(i => i).ToList();
    }

    /// <summary>
    /// Assigns jobs based on the given preferences and list of stations to assign for.
    /// This does NOT change the slots on the station, only figures out where each player should go.
    /// </summary>
    /// <param name="profiles">The profiles to use for selection.</param>
    /// <param name="stations">List of stations to assign for.</param>
    /// <param name="useRoundStartJobs">Whether or not to use the round-start jobs for the stations instead of their current jobs.</param>
    /// <returns>List of players and their assigned jobs.</returns>
    /// <remarks>
    /// You probably shouldn't use useRoundStartJobs mid-round if the station has been available to join,
    /// as there may end up being more round-start slots than available slots, which can cause weird behavior.
    /// A warning to all who enter ye cursed lands: This function is long and mildly incomprehensible. Best used without touching.
    /// </remarks>
    public Dictionary<NetUserId, (ProtoId<JobPrototype>?, EntityUid)> AssignJobs(Dictionary<NetUserId, HumanoidCharacterProfile> profiles, IReadOnlyList<EntityUid> stations, bool useRoundStartJobs = true)
    {
        DebugTools.Assert(stations.Count > 0);

        InitializeRoundStart();

        if (profiles.Count == 0)
            return new();
// CCM priority rework
        var originalProfiles = profiles;

        // We need to modify this collection later, so make a copy of it.
        profiles = profiles.ShallowClone();

        var selectedSlots = new Dictionary<NetUserId, int>(profiles.Count);
        foreach (var userId in profiles.Keys)
        {
            var prefs = _preferences.GetPreferences(userId);
            selectedSlots[userId] = prefs.SelectedCharacterIndex;
        }
        var currentRoundId = _gameTicker.RoundId;
        var sessionMinutes = new Dictionary<NetUserId, float>(profiles.Count);
        foreach (var userId in profiles.Keys)
        {
            if (_player.TryGetSessionById(userId, out var session))
            {
                var minutes = (float) (DateTime.UtcNow - session.ConnectedTime).TotalMinutes;
                sessionMinutes[userId] = MathF.Max(0f, minutes);
            }
            else
            {
                sessionMinutes[userId] = 0f;
            }
        }
        // CCM priority rework
        // Player <-> (job, station)
        var assigned = new Dictionary<NetUserId, (ProtoId<JobPrototype>?, EntityUid)>(profiles.Count);

        // The jobs left on the stations. This collection is modified as jobs are assigned to track what's available.
        var stationJobs = new Dictionary<EntityUid, Dictionary<ProtoId<JobPrototype>, int?>>();
        foreach (var station in stations)
        {
            if (useRoundStartJobs)
            {
                stationJobs.Add(station, GetRoundStartJobs(station).ToDictionary(x => x.Key, x => x.Value));
            }
            else
            {
                stationJobs.Add(station, GetJobs(station).ToDictionary(x => x.Key, x => x.Value));
            }
        }


        // We reuse this collection. It tracks what jobs we're currently trying to select players for.
        var currentlySelectingJobs = new Dictionary<EntityUid, Dictionary<ProtoId<JobPrototype>, int?>>(stations.Count);
        foreach (var station in stations)
        {
            currentlySelectingJobs.Add(station, new Dictionary<ProtoId<JobPrototype>, int?>());
        }

        // And these.
        // Tracks what players are available for a given job in the current iteration of selection.
        var jobPlayerOptions = new Dictionary<ProtoId<JobPrototype>, HashSet<NetUserId>>();
        // Tracks the total number of slots for the given stations in the current iteration of selection.
        var stationTotalSlots = new Dictionary<EntityUid, int>(stations.Count);
        // The share of the players each station gets in the current iteration of job selection.
        var stationShares = new Dictionary<EntityUid, int>(stations.Count);

        // Ok so the general algorithm:
        // We start with the highest weight jobs and work our way down. We filter jobs by weight when selecting as well.
        // Weight > Priority > Station.
        foreach (var weight in _orderedWeights)
        {
            foreach (var selectedPriority in JobPriorityExtensions.OrderedRoundstartPriorities)
            {
                if (profiles.Count == 0)
                    goto endFunc;

                var candidates = GetPlayersJobCandidates(weight, selectedPriority, profiles);

                var optionsRemaining = 0;
                var isFirstOrder = selectedPriority.IsFirst();
                Dictionary<ProtoId<JobPrototype>, Dictionary<NetUserId, float>>? jobWeights =
                    isFirstOrder ? new Dictionary<ProtoId<JobPrototype>, Dictionary<NetUserId, float>>() : null;

                // Assigns a player to the given station, updating all the bookkeeping while at it.
                void AssignPlayer(NetUserId player, ProtoId<JobPrototype> job, EntityUid station)
                {
                    // Remove the player from all possible jobs as that's faster than actually checking what they have selected.
                    foreach (var (k, players) in jobPlayerOptions)
                    {
                        players.Remove(player);
                        if (players.Count == 0)
                            jobPlayerOptions.Remove(k);
                    }

                    stationJobs[station][job]--;
                    profiles.Remove(player);
                    assigned.Add(player, (job, station));

                    optionsRemaining--;
                }
// CCM priority rework
                NetUserId PickPlayerForJob(ProtoId<JobPrototype> job)
                {
                    if (!isFirstOrder || jobWeights == null || !jobWeights.TryGetValue(job, out var weights))
                        return _random.Pick(jobPlayerOptions[job]);

                    return PickWeightedPlayer(jobPlayerOptions[job], weights);
                }
// CCM priority rework
                jobPlayerOptions.Clear(); // We reuse this collection.

                // Goes through every candidate, and adds them to jobPlayerOptions, so that the candidate players
                // have an index sorted by job. We use this (much) later when actually assigning people to randomly
                // pick from the list of candidates for the job.
                foreach (var (user, jobs) in candidates)
                {
                    foreach (var job in jobs)
                    {
                        if (!jobPlayerOptions.ContainsKey(job))
                            jobPlayerOptions.Add(job, new HashSet<NetUserId>());

                        jobPlayerOptions[job].Add(user);
// CCM priority rework
                        if (jobWeights != null)
                        {
                            if (!jobWeights.TryGetValue(job, out var weights))
                            {
                                weights = new Dictionary<NetUserId, float>();
                                jobWeights[job] = weights;
                            }

                            weights[user] = CalculateFirstOrderWeight(user, selectedSlots[user], job, sessionMinutes[user], currentRoundId);
                        }
                    }
// CCM priority rework
                    optionsRemaining++;
                }

                // We reuse this collection, so clear it's children.
                foreach (var slots in currentlySelectingJobs)
                {
                    slots.Value.Clear();
                }

                // Go through every station..
                foreach (var station in stations)
                {
                    var slots = currentlySelectingJobs[station];

                    // Get all of the jobs in the selected weight category.
                    foreach (var (job, slot) in stationJobs[station])
                    {
                        if (_jobsByWeight[weight].Contains(job))
                            slots.Add(job, slot);
                    }
                }


                // Clear for reuse.
                stationTotalSlots.Clear();

                // Intentionally discounts the value of uncapped slots! They're only a single slot when deciding a station's share.
                foreach (var (station, jobs) in currentlySelectingJobs)
                {
                    stationTotalSlots.Add(
                        station,
                        (int)jobs.Values.Sum(x => x ?? 1)
                        );
                }

                var totalSlots = 0;

                // LINQ moment.
                // totalSlots = stationTotalSlots.Sum(x => x.Value);
                foreach (var (_, slot) in stationTotalSlots)
                {
                    totalSlots += slot;
                }

                if (totalSlots == 0)
                    continue; // No slots so just move to the next iteration.

                // Clear for reuse.
                stationShares.Clear();

                // How many players we've distributed so far. Used to grant any remaining slots if we have leftovers.
                var distributed = 0;

                // Goes through each station and figures out how many players we should give it for the current iteration.
                foreach (var station in stations)
                {
                    // Calculates the percent share then multiplies.
                    stationShares[station] = (int)Math.Floor(((float)stationTotalSlots[station] / totalSlots) * candidates.Count);
                    distributed += stationShares[station];
                }

                // Avoids the fair share problem where if there's two stations and one player neither gets one.
                // We do this by simply selecting a station randomly and giving it the remaining share(s).
                if (distributed < candidates.Count)
                {
                    var choice = _random.Pick(stations);
                    stationShares[choice] += candidates.Count - distributed;
                }

                // Actual meat, goes through each station and shakes the tree until everyone has a job.
                foreach (var station in stations)
                {
                    if (stationShares[station] == 0)
                        continue;

                    // The jobs we're selecting from for the current station.
                    var currStationSelectingJobs = currentlySelectingJobs[station];
                    // We only need this list because we need to go through this in a random order.
                    // Oh the misery, another allocation.
                    var allJobs = currStationSelectingJobs.Keys.ToList();
                    _random.Shuffle(allJobs);
                    // And iterates through all it's jobs in a random order until the count settles.
                    // No, AFAIK it cannot be done any saner than this. I hate "shaking" collections as much
                    // as you do but it's what seems to be the absolute best option here.
                    // It doesn't seem to show up on the chart, perf-wise, anyway, so it's likely fine.
                    int priorCount;
                    do
                    {
                        priorCount = stationShares[station];

                        foreach (var job in allJobs)
                        {
                            if (stationShares[station] == 0)
                                break;

                            if (currStationSelectingJobs[job] != null && currStationSelectingJobs[job] == 0)
                                continue; // Can't assign this job.

                            if (!jobPlayerOptions.ContainsKey(job))
                                continue;

                            // Picking players it finds that have the job set.
                            var player = PickPlayerForJob(job);
                            AssignPlayer(player, job, station);
                            stationShares[station]--;

                            if (currStationSelectingJobs[job] != null)
                                currStationSelectingJobs[job]--;

                            if (optionsRemaining == 0)
                                goto done;
                        }
                    } while (priorCount != stationShares[station]);
                }
                done: ;
            }
        }

        endFunc:
        var roundstartJobSlotCounts = GetRoundstartJobSlotCounts();
        UpdateJobPriorityWeights(originalProfiles, assigned, selectedSlots, currentRoundId, roundstartJobSlotCounts); // CCM priority rework
        return assigned;
    }

    /// <summary>
    /// Attempts to assign overflow jobs to any player in allPlayersToAssign that is not in assignedJobs.
    /// </summary>
    /// <param name="assignedJobs">All assigned jobs.</param>
    /// <param name="allPlayersToAssign">All players that might need an overflow assigned.</param>
    /// <param name="profiles">Player character profiles.</param>
    /// <param name="stations">The stations to consider for spawn location.</param>
    public void AssignOverflowJobs(
        ref Dictionary<NetUserId, (ProtoId<JobPrototype>?, EntityUid)> assignedJobs,
        IEnumerable<NetUserId> allPlayersToAssign,
        IReadOnlyDictionary<NetUserId, HumanoidCharacterProfile> profiles,
        IReadOnlyList<EntityUid> stations)
    {
        var givenStations = stations.ToList();
        if (givenStations.Count == 0)
            return; // Don't attempt to assign them if there are no stations.
        // For players without jobs, give them the overflow job if they have that set...
        foreach (var player in allPlayersToAssign)
        {
            if (assignedJobs.ContainsKey(player))
            {
                continue;
            }

            var profile = profiles[player];
            if (profile.PreferenceUnavailable != PreferenceUnavailableMode.SpawnAsOverflow)
            {
                assignedJobs.Add(player, (null, EntityUid.Invalid));
                continue;
            }

            _random.Shuffle(givenStations);

            foreach (var station in givenStations)
            {
                // Pick a random overflow job from that station
                var overflows = GetOverflowJobs(station).ToList();
                _random.Shuffle(overflows);

                // Stations with no overflow slots should simply get skipped over.
                if (overflows.Count == 0)
                    continue;

                // If the overflow exists, put them in as it.
                assignedJobs.Add(player, (overflows[0], givenStations[0]));
                break;
            }
        }
    }

    public void CalcExtendedAccess(Dictionary<EntityUid, int> jobsCount)
    {
        // Calculate whether stations need to be on extended access or not.
        foreach (var (station, count) in jobsCount)
        {
            var jobs = Comp<StationJobsComponent>(station);

            var thresh = jobs.ExtendedAccessThreshold;

            jobs.ExtendedAccess = count <= thresh;

            Log.Debug("Station {Station} on extended access: {ExtendedAccess}",
                Name(station), jobs.ExtendedAccess);
        }
    }

    /// <summary>
    /// Gets all jobs that the input players have that match the given weight and priority.
    /// </summary>
    /// <param name="weight">Weight to find, if any.</param>
    /// <param name="selectedPriority">Priority to find, if any.</param>
    /// <param name="profiles">Profiles to look in.</param>
    /// <returns>Players and a list of their matching jobs.</returns>
    private Dictionary<NetUserId, List<string>> GetPlayersJobCandidates(int? weight, JobPriority? selectedPriority, Dictionary<NetUserId, HumanoidCharacterProfile> profiles)
    {
        var outputDict = new Dictionary<NetUserId, List<string>>(profiles.Count);

        foreach (var (player, profile) in profiles)
        {
            var roleBans = _banManager.GetJobBans(player);
            var antagBlocked = _antag.GetPreSelectedAntagSessions();
            var profileJobs = profile.JobPriorities.Keys.Select(k => new ProtoId<JobPrototype>(k)).ToList();
            var ev = new StationJobsGetCandidatesEvent(player, profileJobs);
            RaiseLocalEvent(ref ev);

            List<string>? availableJobs = null;

            foreach (var jobId in profileJobs)
            {
                var priority = profile.JobPriorities[jobId];

                if (selectedPriority != null)
                {
                    if (selectedPriority.Value.IsFirst() && !priority.IsFirst())
                        continue;
                    if (selectedPriority.Value.IsSecond() && !priority.IsSecond())
                        continue;
                }

                if (!_prototypeManager.TryIndex(jobId, out var job))
                    continue;

                if (!job.CanBeAntag && (!_player.TryGetSessionById(player, out var session) || antagBlocked.Contains(session)))
                    continue;

                if (weight is not null && job.Weight != weight.Value)
                    continue;

                if (!(roleBans == null || !roleBans.Contains(jobId)))
                    continue;

                availableJobs ??= new List<string>(profile.JobPriorities.Count);
                availableJobs.Add(jobId);
            }

            if (availableJobs is not null)
                outputDict.Add(player, availableJobs);
        }

        return outputDict;
    }
// # CCM priority rework-start

    private void UpdateJobPriorityWeights(
        IReadOnlyDictionary<NetUserId, HumanoidCharacterProfile> profiles,
        IReadOnlyDictionary<NetUserId, (ProtoId<JobPrototype>?, EntityUid)> assigned,
        IReadOnlyDictionary<NetUserId, int> selectedSlots,
        int roundId,
        IReadOnlyDictionary<ProtoId<JobPrototype>, int> roundstartJobSlotCounts)
    {
        foreach (var (userId, profile) in profiles)
        {
            var firstOrderJobs = profile.JobPriorities
                .Where(pair => pair.Value.IsFirst())
                .Select(pair => pair.Key)
                .ToList();

            if (firstOrderJobs.Count == 0)
                continue;

            if (!selectedSlots.TryGetValue(userId, out var slot))
                continue;

            assigned.TryGetValue(userId, out var assignedJob);
            _jobPriorityWeights.ApplyRoundResults(userId, slot, firstOrderJobs, assignedJob.Item1, roundId, roundstartJobSlotCounts);
        }
    }

    private const float BaseFirstOrderWeight = 1f;
    private const float EarlyMissedRoundWeight = 0.35f;
    private const float MidMissedRoundWeight = 0.30f;
    private const float LateMissedRoundWeight = 0.25f;
    private const float EndlessMissedRoundWeight = 0.15f;
    private const float RecentRolePenalty = -0.5f;
    private const float SessionMinutesPerBonus = 30f;
    private const float SessionBonusPerStep = 0.15f;
    private const float MaxSessionBonus = 1.8f;
    private const float MinFirstOrderWeight = 0.25f;

    private readonly Dictionary<(NetUserId UserId, int Slot, ProtoId<JobPrototype> JobId), FirstOrderWeightOverride>
        _firstOrderWeightOverrides = new();
    private readonly Dictionary<NetUserId, float> _sessionMinutesOverrides = new();
    private readonly Dictionary<NetUserId, float> _externalBonusOverrides = new();

    public readonly record struct FirstOrderChanceReportEntry(
        ProtoId<JobPrototype> JobId,
        float ChancePercent,
        float Weight,
        float TotalWeight,
        FirstOrderWeightBreakdown Breakdown);

    public bool TryGetFirstOrderChanceReport(
        NetUserId userId,
        HumanoidCharacterProfile profile,
        int slot,
        out List<FirstOrderChanceReportEntry> report,
        out string? warning)
    {
        report = new List<FirstOrderChanceReportEntry>();
        warning = null;

        var selectedProfiles = new Dictionary<NetUserId, HumanoidCharacterProfile>();
        var selectedSlots = new Dictionary<NetUserId, int>();
        var sessionMinutes = new Dictionary<NetUserId, float>();

        foreach (var session in _player.Sessions)
        {
            if (!_preferences.TryGetCachedPreferences(session.UserId, out var prefs))
                continue;

            if (prefs.SelectedCharacter is HumanoidCharacterProfile selectedProfile)
                selectedProfiles[session.UserId] = selectedProfile;

            selectedSlots[session.UserId] = prefs.SelectedCharacterIndex;
            sessionMinutes[session.UserId] = MathF.Max(0f, (float) (DateTime.UtcNow - session.ConnectedTime).TotalMinutes);
        }

        if (selectedProfiles.Count == 0)
        {
            warning = "No lobby profiles are available.";
            return false;
        }

        var profiles = new Dictionary<NetUserId, HumanoidCharacterProfile>(selectedProfiles)
        {
            [userId] = profile
        };

        var slots = new Dictionary<NetUserId, int>(selectedSlots)
        {
            [userId] = slot
        };

        var currentRoundId = _gameTicker.RoundId;
        var jobSlotCounts = GetRoundstartJobSlotCounts();
        var candidates = GetPlayersJobCandidates(null, JobPriority.First, profiles);

        if (!candidates.TryGetValue(userId, out var userJobs) || userJobs.Count == 0)
        {
            warning = "Player has no first-order jobs.";
            return false;
        }

        var jobWeights = new Dictionary<ProtoId<JobPrototype>, Dictionary<NetUserId, float>>();
        var jobTotals = new Dictionary<ProtoId<JobPrototype>, float>();

        foreach (var (user, jobs) in candidates)
        {
            foreach (var job in jobs)
            {
                if (!jobWeights.TryGetValue(job, out var weights))
                {
                    weights = new Dictionary<NetUserId, float>();
                    jobWeights[job] = weights;
                }

                var weight = CalculateFirstOrderWeight(user, slots[user], job, sessionMinutes.GetValueOrDefault(user), currentRoundId);
                weights[user] = weight;
                jobTotals[job] = jobTotals.GetValueOrDefault(job) + weight;
            }
        }

        foreach (var jobId in userJobs)
        {
            if (!jobTotals.TryGetValue(jobId, out var total) || total <= 0f)
                continue;

            var weight = jobWeights[jobId][userId];
            var slotCount = jobSlotCounts.GetValueOrDefault(jobId, 1);
            if (slotCount <= 0)
                continue;

            var chance = MathF.Min(100f, weight / total * 100f * slotCount);
            var breakdown = CalculateFirstOrderWeightBreakdown(
                userId,
                slot,
                jobId,
                sessionMinutes.GetValueOrDefault(userId),
                currentRoundId);

            report.Add(new FirstOrderChanceReportEntry(
                jobId,
                chance,
                weight,
                total,
                breakdown));
        }

        return report.Count > 0;
    }

    private float CalculateFirstOrderWeight(
        NetUserId user,
        int slot,
        ProtoId<JobPrototype> jobId,
        float sessionMinutes,
        int currentRoundId)
    {
        var weight = BaseFirstOrderWeight;
        var (missedRounds, assignedLastRound) = GetFirstOrderWeightInputs(user, slot, jobId, currentRoundId);
        weight += CalculateMissedRoundsWeight(missedRounds);
        if (assignedLastRound)
            weight += RecentRolePenalty;

        sessionMinutes = GetSessionMinutesOverride(user, sessionMinutes);
        var sessionBonusSteps = (int) MathF.Floor(MathF.Min(sessionMinutes, 360f) / SessionMinutesPerBonus);
        var sessionBonus = sessionBonusSteps > 0 ? MathF.Min(sessionBonusSteps * SessionBonusPerStep, MaxSessionBonus) : 0f;
        if (sessionBonus > 0f)
            weight += sessionBonus;

        weight += GetExternalWeightModifier(user);
        return MathF.Max(weight, MinFirstOrderWeight);
    }

    private FirstOrderWeightBreakdown CalculateFirstOrderWeightBreakdown(
        NetUserId user,
        int slot,
        ProtoId<JobPrototype> jobId,
        float sessionMinutes,
        int currentRoundId)
    {
        var baseWeight = BaseFirstOrderWeight;
        var (missedRounds, assignedLastRound) = GetFirstOrderWeightInputs(user, slot, jobId, currentRoundId);
        var missedWeight = CalculateMissedRoundsWeight(missedRounds);
        var recentPenalty = assignedLastRound ? RecentRolePenalty : 0f;

        sessionMinutes = GetSessionMinutesOverride(user, sessionMinutes);
        var sessionBonusSteps = (int) MathF.Floor(MathF.Min(sessionMinutes, 360f) / SessionMinutesPerBonus);
        var sessionBonus = sessionBonusSteps > 0 ? MathF.Min(sessionBonusSteps * SessionBonusPerStep, MaxSessionBonus) : 0f;

        var externalBonus = GetExternalWeightModifier(user);
        var total = baseWeight + missedWeight + recentPenalty + sessionBonus + externalBonus;
        if (total < MinFirstOrderWeight)
            total = MinFirstOrderWeight;

        return new FirstOrderWeightBreakdown(
            baseWeight,
            missedRounds,
            missedWeight,
            recentPenalty,
            sessionBonusSteps,
            sessionBonus,
            externalBonus,
            total);
    }

    private float CalculateMissedRoundsWeight(int missedRounds)
    {
        var effectiveMissedRounds = Math.Max(missedRounds, 0);
        var weight = 0f;

        for (var i = 0; i < effectiveMissedRounds; i++)
        {
            weight += i switch
            {
                < 3 => EarlyMissedRoundWeight,
                < 6 => MidMissedRoundWeight,
                < 15 => LateMissedRoundWeight,
                _ => EndlessMissedRoundWeight,
            };
        }

        return weight;
    }

    public readonly record struct FirstOrderWeightBreakdown(
        float BaseWeight,
        int MissedRounds,
        float MissedWeight,
        float RecentPenalty,
        int SessionBonusSteps,
        float SessionBonus,
        float ExternalBonus,
        float TotalWeight);

    public void SetFirstOrderWeightOverride(
        NetUserId userId,
        int slot,
        ProtoId<JobPrototype> jobId,
        int missedRounds,
        bool assignedLastRound)
    {
        _firstOrderWeightOverrides[(userId, slot, jobId)] =
            new FirstOrderWeightOverride(missedRounds, assignedLastRound);
    }

    public bool ClearFirstOrderWeightOverride(NetUserId userId, int slot, ProtoId<JobPrototype> jobId)
    {
        return _firstOrderWeightOverrides.Remove((userId, slot, jobId));
    }

    public void ClearFirstOrderWeightOverrides(NetUserId userId)
    {
        var keys = _firstOrderWeightOverrides.Keys
            .Where(key => key.UserId == userId)
            .ToList();

        foreach (var key in keys)
        {
            _firstOrderWeightOverrides.Remove(key);
        }
    }

    public void SetSessionMinutesOverride(NetUserId userId, float sessionMinutes)
    {
        _sessionMinutesOverrides[userId] = MathF.Max(0f, sessionMinutes);
    }

    public void ClearSessionMinutesOverride(NetUserId userId)
    {
        _sessionMinutesOverrides.Remove(userId);
    }

    public void SetExternalWeightOverride(NetUserId userId, float bonus)
    {
        _externalBonusOverrides[userId] = bonus;
    }

    public void ClearExternalWeightOverride(NetUserId userId)
    {
        _externalBonusOverrides.Remove(userId);
    }

    private (int MissedRounds, bool AssignedLastRound) GetFirstOrderWeightInputs(
        NetUserId user,
        int slot,
        ProtoId<JobPrototype> jobId,
        int currentRoundId)
    {
        if (_firstOrderWeightOverrides.TryGetValue((user, slot, jobId), out var overrideValues))
            return (overrideValues.MissedRounds, overrideValues.AssignedLastRound);

        if (_jobPriorityWeights.TryGetWeight(user, slot, jobId, out var record) && record != null)
        {
            var assignedLastRound = record.LastAssignedRoundId.HasValue &&
                record.LastAssignedRoundId.Value == currentRoundId - 1;
            return (record.MissedRounds, assignedLastRound);
        }

        return (0, false);
    }

    private float GetSessionMinutesOverride(NetUserId user, float sessionMinutes)
    {
        return _sessionMinutesOverrides.TryGetValue(user, out var overrideMinutes)
            ? overrideMinutes
            : sessionMinutes;
    }

    private float GetExternalWeightModifier(NetUserId user)
    {
        if (_externalBonusOverrides.TryGetValue(user, out var overrideBonus))
            return overrideBonus;

        // Placeholder for donate/admin modifiers.
        return 0f;
    }

    private Dictionary<ProtoId<JobPrototype>, int> GetRoundstartJobSlotCounts()
    {
        var counts = new Dictionary<ProtoId<JobPrototype>, int>();
        var query = EntityQueryEnumerator<StationJobsComponent>();
        while (query.MoveNext(out var uid, out var stationJobs))
        {
            var jobs = GetRoundStartJobs(uid, stationJobs);
            foreach (var (jobId, slot) in jobs)
            {
                var count = slot ?? 1;
                if (count <= 0)
                    continue;

                counts[jobId] = counts.GetValueOrDefault(jobId) + count;
            }
        }

        return counts;
    }

    private readonly record struct FirstOrderWeightOverride(
        int MissedRounds,
        bool AssignedLastRound);

    private NetUserId PickWeightedPlayer(IReadOnlyCollection<NetUserId> candidates, Dictionary<NetUserId, float> weights)
    {
        var totalWeight = 0f;
        foreach (var candidate in candidates)
        {
            totalWeight += weights.TryGetValue(candidate, out var weight) ? weight : MinFirstOrderWeight;
        }

        if (totalWeight <= 0f)
            return candidates.First();

        var roll = _random.NextFloat() * totalWeight;
        foreach (var candidate in candidates)
        {
            var weight = weights.TryGetValue(candidate, out var value) ? value : MinFirstOrderWeight;
            roll -= weight;
            if (roll <= 0f)
                return candidate;
        }

        return candidates.First();
    }
}

// # CCM priority rework-end
