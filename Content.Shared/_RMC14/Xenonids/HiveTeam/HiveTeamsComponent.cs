using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Xenonids.HiveTeam;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HiveTeamsComponent : Component
{
    public const int TeamCount = 4;
    public static readonly string[] RoleNames = ["rmc-hive-role-maim", "rmc-hive-role-rip", "rmc-hive-role-tear", "rmc-hive-role-follow-queen", "rmc-hive-role-build", "rmc-hive-role-backline", "rmc-hive-role-capture", "rmc-hive-role-stall"];

    [DataField, AutoNetworkedField]
    public List<HiveTeamEntry> Teams = [];
}

[Serializable, NetSerializable]
public sealed class HiveTeamEntry
{
    [DataField]
    public NetEntity? Leader;
    [DataField]
    public List<NetEntity> Members = [];
    [DataField]
    public int Role;
}
