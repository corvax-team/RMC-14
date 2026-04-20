using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.Abilities.Runi.Charge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMXenoChargeLineActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public int MaxTiles;

    [DataField, AutoNetworkedField]
    public float HitRadius;

    [DataField, AutoNetworkedField]
    public float HealPerHit;

    public int TilesTraveled;

    public HashSet<EntityUid> HitEntities = new();

    [DataField]
    public bool HealTriggered = false;
}
