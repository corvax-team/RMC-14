using System.Collections.Generic;
using Content.Shared.Preferences;

namespace Content.Shared._CCM.Preferences;

public static class JobPriorityExtensions
{
    public static readonly IReadOnlyList<JobPriority> OrderedRoundstartPriorities =
        new[] { JobPriority.First, JobPriority.Second };

    public static bool IsFirst(this JobPriority priority)
    {
        return priority == JobPriority.First;
    }

    public static bool IsSecond(this JobPriority priority)
    {
        return priority is JobPriority.Second or JobPriority.SecondFallback;
    }

    public static JobPriority NormalizeSecond(this JobPriority priority)
    {
        return priority == JobPriority.SecondFallback ? JobPriority.Second : priority;
    }
}

// # CCM priority rework
