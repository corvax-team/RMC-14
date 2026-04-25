// CM14 rework: non-RMC edit marker.
using System.Collections.Generic;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Barks;

[Prototype("bark")]
public sealed partial class BarkPrototype : IPrototype
{
    public const string Default = "BarkMaleVoice01";

    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("soundFiles", required: true)]
    public List<string> SoundFiles { get; private set; } = new();

    [DataField("roundStart")]
    public bool RoundStart { get; private set; } = true;

    [DataField("voiceType")]
    public BarkVoiceType VoiceType { get; private set; } = BarkVoiceType.Normal;
}

public enum BarkVoiceType : byte
{
    Normal,
    Robot,
    Alien,
    Creature,
}

public enum VoicePitchPreset : byte
{
    Low,
    Medium,
    High,
}
