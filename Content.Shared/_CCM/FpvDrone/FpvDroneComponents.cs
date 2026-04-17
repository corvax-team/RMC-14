using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.FpvDrone;

public static class FpvDroneConstants
{
    public const string ShaderId = "FpvDroneShader";
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FpvDroneComponent : Component
{
    [DataField] public EntityUid Control;
    [DataField] public EntityUid? EjectAction;
    [DataField] public string EjectActionPrototypeId = "ActionFpvDroneEject";

    [DataField] public SoundSpecifier? FlyingLoopSound =
        new SoundPathSpecifier("/Audio/_CCM14/FpvDrone/drone_fly_loop.ogg");

    public EntityUid? FlyingStream;
    [DataField] [AutoNetworkedField] public float MaxRange = 50f;
    [DataField] public EntityUid? Pilot;
    [DataField] [AutoNetworkedField] public bool SignalLost;

    [DataField] public SoundSpecifier? SignalLostSound =
        new SoundPathSpecifier("/Audio/_CCM14/FpvDrone/drone_signal_lost.ogg");
}

[RegisterComponent]
public sealed partial class FpvDroneGogglesComponent : Component
{
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FpvDroneExplosiveComponent : Component
{
    [DataField] public EntityUid? ExplodeActionEntity;
    [DataField] public EntProtoId? ExplodeActionId = "ActionFpvDroneExplosive";
    [DataField] [AutoNetworkedField] public float Radius = 5f;
    [DataField] [AutoNetworkedField] public float TotalIntensity = 100f;
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FpvDroneFoldableComponent : Component
{
    [DataField] [AutoNetworkedField] public float UnfoldDelay = 1.5f;

    [DataField] [AutoNetworkedField] public EntProtoId UnfoldEntity = "FpvDroneObserver";
}
