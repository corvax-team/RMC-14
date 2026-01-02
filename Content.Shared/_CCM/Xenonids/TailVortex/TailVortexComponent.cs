using Content.Shared._RMC14.Pulling;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.TailVortex;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TailVortexSystem))]
public sealed partial class TailVortexComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 250;

    [DataField, AutoNetworkedField]
    public int GuaranteedQuantity = 3;

    [DataField, AutoNetworkedField]
    public int StacksCount = 0;

    [DataField, AutoNetworkedField]
    public int MaxStacks = 4;

    [DataField, AutoNetworkedField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan VortexDelay = TimeSpan.FromSeconds(0.8);

    [DataField, AutoNetworkedField]
    public float Range = 2f;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public float Power = 2.5f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("XenoTailSwipe")
    {
        Params = AudioParams.Default.WithVariation(0.15f),
    };
}
