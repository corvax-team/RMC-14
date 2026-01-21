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
    public Dictionary<ProtoId<JobPrototype>, float> Chances { get; }

    public JobPriorityChancesEvent(int characterSlot, Dictionary<ProtoId<JobPrototype>, float> chances)
    {
        CharacterSlot = characterSlot;
        Chances = chances;
    }
}

// # CCM priority rework
