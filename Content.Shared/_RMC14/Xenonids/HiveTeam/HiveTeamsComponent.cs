namespace Content.Shared._RMC14.Xenonids.HiveTeam;

[RegisterComponent]
public sealed partial class HiveTeamsComponent : Component
{
    public const int TeamCount = 4;
    public static readonly string[] RoleNames =
    [
        "rmc-hive-role-maim",
        "rmc-hive-role-rip",
        "rmc-hive-role-tear",
        "rmc-hive-role-follow-queen",
        "rmc-hive-role-build",
        "rmc-hive-role-backline",
        "rmc-hive-role-capture",
        "rmc-hive-role-stall"
    ];

    public List<HiveTeamEntry> Teams = [];
}

public sealed class HiveTeamEntry
{
    public EntityUid? Leader;
    public List<EntityUid> Members = [];
    public int Role = 0;
}
