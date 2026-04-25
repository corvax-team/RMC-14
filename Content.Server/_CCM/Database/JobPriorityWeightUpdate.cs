namespace Content.Server._CCM.Database;

public readonly record struct JobPriorityWeightUpdate(
    string JobId,
    int MissedRounds,
    int? LastAssignedRoundId);

// # CCM priority rework
