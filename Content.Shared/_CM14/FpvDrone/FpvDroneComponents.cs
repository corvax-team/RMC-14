using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CM14.FpvDrone;

public static class FpvDroneConstants
{
    public const string ShaderId = "FpvDroneShader";
    public const string FontPath = "/Fonts/NotoSans/NotoSans-Bold.ttf";
}

[RegisterComponent]
public sealed partial class FpvDroneControlComponent : Component
{
    [DataField] public EntityUid? Observer;
    [DataField] public string ObserverPrototypeId = "FpvDroneObserver";
    [DataField] public EntityUid? Pilot;
    [DataField] public bool Used;
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FpvDroneObserverComponent : Component
{
    [DataField] public EntityUid Control;
    [DataField] public EntityUid? EjectAction;
    [DataField] public string EjectActionPrototypeId = "ActionFpvDroneEject";

    [DataField] public SoundSpecifier? FlyingLoopSound =
        new SoundPathSpecifier("/Audio/_CCM14/FpvDrone/drone_fly_loop.ogg");

    public EntityUid? FlyingStream;
    [DataField] [AutoNetworkedField] public float MaxRange = 50f;
    [DataField] public EntityUid? Pilot;

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
public sealed partial class FpvDroneScreenOverlayComponent : Component
{
    [DataField] [AutoNetworkedField] public bool SignalLost;
    [DataField] public float TimeUntilExplosion = 1.0f;
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