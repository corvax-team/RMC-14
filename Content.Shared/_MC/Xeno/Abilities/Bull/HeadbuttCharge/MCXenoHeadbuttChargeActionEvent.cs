using Content.Shared.Actions;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

public sealed partial class MCXenoHeadbuttChargeActionEvent : InstantActionEvent
{
    [DataField]
    public bool Collide = true;

    [DataField]
    public float Knockback;

    [DataField]
    public float KnockbackSpeed;

    [DataField]
    public TimeSpan Paralyze;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);

    [DataField]
    public DamageSpecifier? Damage;

    [DataField]
    public float DamageMultiplier;

    [DataField]
    public float SpeedMultiplier = 1.45f;

    [DataField]
    public EntProtoId? TurfSpawnEntityId;

    [DataField]
    public ProtoId<EmotePrototype> ActivationEmote = "XenoRoar";

    [DataField]
    public SoundSpecifier? HitSound;

    [DataField]
    public SoundSpecifier? FootstepSound;

    [DataField]
    public FixedPoint2 PlasmaCost = 40;
}
