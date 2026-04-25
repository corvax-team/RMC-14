// CM14 rework: non-RMC edit marker.
using System.Diagnostics.CodeAnalysis;
using Content.Shared._RMC14.Roles;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

public static class JobRequirements
{
    public static bool TryRequirementsMet(
        JobPrototype job,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        bool ignorePlaytimeRequirements = false)
    {
        var sys = entManager.System<SharedRoleSystem>();
        var requirements = sys.GetJobRequirement(job);
        reason = null;
        if (requirements == null)
            return true;

        // CCM sponsorship playtime bypass start
        foreach (var requirement in requirements)
        {
            if (ignorePlaytimeRequirements && IsPlaytimeRequirement(requirement))
                continue;

            if (!requirement.Check(entManager, protoManager, profile, playTimes, out reason))
                return false;
        }
        // CCM sponsorship playtime bypass end

        return true;
    }

    // CCM sponsorship playtime bypass: shared filter for timer-based role requirements.
    public static bool IsPlaytimeRequirement(JobRequirement requirement)
    {
        return requirement is RoleTimeRequirement
            or DepartmentTimeRequirement
            or OverallPlaytimeRequirement
            or TotalJobsTimeRequirement
            or TotalDepartmentsTimeRequirement;
    }
}

/// <summary>
/// Abstract class for playtime and other requirements for role gates.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public abstract partial class JobRequirement
{
    [DataField]
    public bool Inverted;

    public abstract bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason);
}
