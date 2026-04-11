using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHeadbuttChargeActiveComponent : Component
{
    public const int FootstepTurfsCount = 2;

    [DataField, AutoNetworkedField]
    public bool Collide;

    [DataField, AutoNetworkedField]
    public float Knockback;

    [DataField, AutoNetworkedField]
    public float KnockbackSpeed;

    [DataField, AutoNetworkedField]
    public TimeSpan Paralyze;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? HitSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? FootstepSound;

    [DataField, AutoNetworkedField]
    public DamageSpecifier? Damage;

    [DataField, AutoNetworkedField]
    public float DamageMultiplier;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier;

    [DataField, AutoNetworkedField]
    public EntProtoId? TurfSpawnEntityId;

    [DataField, AutoNetworkedField]
    public TimeSpan? Duration;

    [DataField, AutoNetworkedField]
    public float DurationElapsed;

    /*
     * Cache data
     */

    [DataField, AutoNetworkedField]
    public int FootstepTurfAccumulator;

    [DataField, AutoNetworkedField]
    public Vector2i? LastTurf;
}
