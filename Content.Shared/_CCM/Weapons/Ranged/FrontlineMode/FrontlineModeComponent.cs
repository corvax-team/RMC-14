using Content.Shared._RMC14.Projectiles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Weapons.Ranged.Frontline;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SmartGunFrontlineSystem))]
public sealed partial class SmartGunFrontlineComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = false;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "CCMActionToggleFrontline";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public SoundSpecifier ToggleSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/click.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier BlockSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/buzz_two.ogg");

    [DataField, AutoNetworkedField]
    public float MaxDistance = 12f;

    [DataField, AutoNetworkedField]
    public LocId BlockedMessage = "ccm-smartgun-iff-blocked";

    [DataField, AutoNetworkedField]
    public List<DamageFalloffThreshold> AltFalloffThresholds = new();

    [DataField, AutoNetworkedField]
    public TimeSpan BlockMessageCooldown = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan NextBlockMessageTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public Angle BaseConeAngle = Angle.FromDegrees(10);
}
