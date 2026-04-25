// CM14 rework: non-RMC edit marker.
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Barks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeechSynthesisComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<BarkPrototype>? VoicePrototypeId = BarkPrototype.Default;

    [DataField, AutoNetworkedField]
    public float PlaybackSpeed = 1f;

    [DataField, AutoNetworkedField]
    public float Pitch = 1f;

    [DataField, AutoNetworkedField]
    public float Expression = 1f;

    [DataField, AutoNetworkedField]
    public VoicePitchPreset PitchPreset = VoicePitchPreset.Medium;

    [DataField, AutoNetworkedField]
    public float BaseVolume = 0f;
}
