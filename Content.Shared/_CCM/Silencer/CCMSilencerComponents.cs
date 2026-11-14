using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Silencer;

/// <summary>
///     Size class used by Silencer gear: Small = T1 and below, Medium = T2, Large = T3+ or big-sized xenos.
/// </summary>
public enum CCMSonicXenoClass : byte
{
    Small,
    Medium,
    Large,
}

#region Sonic carbine

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CCMSonicCarbineSystem))]
public sealed partial class CCMSonicRoundComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SlowPerStackSmall = 0.10f;

    [DataField, AutoNetworkedField]
    public float SlowPerStackLarge = 0.05f;

    [DataField, AutoNetworkedField]
    public int MaxStacks = 3;

    [DataField, AutoNetworkedField]
    public TimeSpan StackDuration = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public TimeSpan HiveDisruptDuration = TimeSpan.FromSeconds(5);
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CCMSonicCarbineSystem))]
public sealed partial class CCMSonicStacksComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<TimeSpan> Stacks = new();

    [DataField, AutoNetworkedField]
    public float SlowPerStack = 0.10f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CCMSonicCarbineSystem))]
public sealed partial class CCMHiveDisruptedComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan ExpiresAt;
}

#endregion

#region Impulse underbarrel emitter

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CCMSonicPulseSystem))]
public sealed partial class CCMSonicPulseComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 3.5f;

    [DataField, AutoNetworkedField]
    public Angle ConeAngle = Angle.FromDegrees(70);

    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public int ArmorPiercing;

    [DataField, AutoNetworkedField]
    public TimeSpan SmallStunTime = TimeSpan.FromSeconds(1.4);

    [DataField, AutoNetworkedField]
    public TimeSpan SmallSlowTime = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan SmallSuperSlowTime = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public float KnockBackPower = 1f;

    [DataField, AutoNetworkedField]
    public float KnockBackSpeed = 5f;

    [DataField, AutoNetworkedField]
    public TimeSpan MediumSlowTime = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan MediumSuperSlowTime = TimeSpan.FromSeconds(1.5);

    [DataField, AutoNetworkedField]
    public TimeSpan LargeSlowTime = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan LargeSuperSlowTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public float WaveDuration = 0.45f;
}

#endregion

#region False Hive grenade

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CCMFalseHiveSystem))]
public sealed partial class CCMFalseHiveCloudComponent : Component
{
    [DataField, AutoNetworkedField]
    public int TileRadius = 1;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 0.8f;

    [DataField, AutoNetworkedField]
    public TimeSpan Linger = TimeSpan.FromSeconds(0.75);

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.25);

    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [DataField, AutoNetworkedField]
    public EntProtoId? VisualProto = "CCMFalseHivePheromoneTile";

    [ViewVariables]
    public EntityUid? Grid;

    [ViewVariables]
    public HashSet<Vector2i> Tiles = new();

    [ViewVariables]
    public List<EntityUid> Visuals = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CCMFalseHiveSystem))]
public sealed partial class CCMFalseHiveSlowedComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 0.8f;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpiresAt;
}

#endregion

#region Echo jammer pack

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMEchoJammerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField, AutoNetworkedField]
    public float HalfExtent = 2.5f;

    [DataField, AutoNetworkedField]
    public float WaveMaxLength = 5f;

    // A full cell of any capacity lasts this long: drain scales with the cell's max charge.
    [DataField, AutoNetworkedField]
    public TimeSpan RunTime = TimeSpan.FromMinutes(8);

    [DataField, AutoNetworkedField]
    public TimeSpan JamLinger = TimeSpan.FromSeconds(0.75);

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.25);

    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "CCMActionToggleEchoJammer";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ToggleOnSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/click.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ToggleOffSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/click.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EmptySound = new SoundPathSpecifier("/Audio/_RMC14/Machines/buzz_two.ogg");
}

/// <summary>
///     Marks a marine inside an active Echo zone. Hiding itself is computed client-side.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CCMEchoJammedComponent : Component
{
    [DataField]
    public TimeSpan ExpiresAt;
}

#endregion
