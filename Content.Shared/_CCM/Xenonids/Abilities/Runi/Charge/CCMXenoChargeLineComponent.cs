using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.Abilities.Runi.Charge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMXenoChargeLineComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan ActivationDelay = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 2.5f;

    [DataField, AutoNetworkedField]
    public int MaxTiles = 10;

    [DataField, AutoNetworkedField]
    public float HitRadius = 3f;

    [DataField, AutoNetworkedField]
    public float HealPerHit = 25f;

    [DataField]
    public SoundSpecifier? HitSound;

    [DataField, AutoNetworkedField]
    public string? AttackEffect;

    [DataField, AutoNetworkedField]
    public string? HealEffect;

    [DataField, AutoNetworkedField]
    public string? Emote;

    [DataField, AutoNetworkedField]
    public TimeSpan? EmoteDelay = TimeSpan.FromSeconds(2);
}
