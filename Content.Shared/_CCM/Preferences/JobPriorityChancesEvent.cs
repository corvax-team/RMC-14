using System;
using System.Collections.Generic;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Preferences;

[Serializable, NetSerializable]
public sealed class JobPriorityChancesEvent : EntityEventArgs
{
    public int CharacterSlot { get; }
    public Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo> Chances { get; }

    public JobPriorityChancesEvent(int characterSlot, Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo> chances)
    {
        CharacterSlot = characterSlot;
        Chances = chances;
    }
}

[Serializable, NetSerializable]
public readonly record struct JobPriorityChanceInfo(
    float ChancePercent,
    float BaseWeight,
    float MissedWeight,
    float RecentPenalty,
    float SessionHours);

// # CCM priority rework
