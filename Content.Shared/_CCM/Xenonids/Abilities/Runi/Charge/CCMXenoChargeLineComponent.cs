using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.Abilities.Runi.Charge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMXenoChargeLineComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan ActivationDelay = TimeSpan.FromSeconds(0.5);
    // Дамаг
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();
    // увелечение скорости при использовании способности
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 2.5f;
    // макс дальность тайлов
    [DataField, AutoNetworkedField]
    public int MaxTiles = 10;

    // радиус впереди
    [DataField, AutoNetworkedField]
    public float HitRadius = 3f;

    // хил за цель
    [DataField, AutoNetworkedField]
    public float HealPerHit = 50f;

    // звук
    [DataField]
    public SoundSpecifier? HitSound;
}