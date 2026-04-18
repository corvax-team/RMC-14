using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Xenonids.TailWhirlwind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TailWhirlwindSystem))]
public sealed partial class TailWhirlwindComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan EndAt;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDuration = TimeSpan.FromSeconds(0.25);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextUpdateAt;

    [DataField, AutoNetworkedField]
    public float Range = 2.5f;

    [DataField, AutoNetworkedField]
    public float ThrowDistance = 2.75f;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("XenoTailSwipe")
    {
        Params = AudioParams.Default.WithVariation(0.15f),
    };
}
