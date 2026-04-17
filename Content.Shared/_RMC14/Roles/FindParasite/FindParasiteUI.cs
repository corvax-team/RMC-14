using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Roles.FindParasite;

[Serializable, NetSerializable]
public sealed partial class FollowParasiteSpawnerMessage : BoundUserInterfaceMessage
{
    public NetEntity Spawner;
    public FollowParasiteSpawnerMessage(NetEntity spawner)
    {
        Spawner = spawner;
    }
}

[Serializable, NetSerializable]
public sealed partial class TakeParasiteRoleMessage : BoundUserInterfaceMessage
{
    public NetEntity Entity;
    public NetEntity Spawner;
    public bool IsRoyalParasite;
    public TakeParasiteRoleMessage(NetEntity entity, NetEntity spawner, bool isRoyalParasite = false)
    {
        Entity = entity;
        Spawner = spawner;
        IsRoyalParasite = isRoyalParasite;
    }
}

[Serializable, NetSerializable]
public sealed partial class RefreshActiveParasiteSpawnersMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed partial class FindParasiteUIState : BoundUserInterfaceState
{
    public List<SpawnerData> ActiveParasiteSpawners = new();
}

[Serializable, NetSerializable]
public sealed partial class SpawnerData
{
    public string Name;
    public NetEntity Spawner;
    public bool IsRoyalParasite;

    public SpawnerData(string name, NetEntity spawner, bool isRoyalParasite = false)
    {
        Name = name;
        Spawner = spawner;
        IsRoyalParasite = isRoyalParasite;
    }
}

[Serializable, NetSerializable]
public enum XenoFindParasiteUI : byte
{
    Key
}
