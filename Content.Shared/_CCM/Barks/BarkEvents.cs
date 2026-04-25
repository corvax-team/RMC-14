// CM14 rework: non-RMC edit marker.
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Barks;

[Serializable, NetSerializable]
public sealed class RequestPreviewBarkEvent : EntityEventArgs
{
    public readonly string BarkVoiceId;
    public readonly float Pitch;
    public readonly float PlaybackSpeed;

    public RequestPreviewBarkEvent(string barkVoiceId, float pitch, float playbackSpeed)
    {
        BarkVoiceId = barkVoiceId;
        Pitch = pitch;
        PlaybackSpeed = playbackSpeed;
    }
}

[Serializable, NetSerializable]
public sealed class PlayBarkEvent : EntityEventArgs
{
    public readonly string SoundPath;
    public readonly NetEntity SourceUid;
    public readonly string Message;
    public readonly float PlaybackSpeed;
    public readonly float Pitch;
    public readonly bool IsObfuscated;
    public readonly bool IsWhisper;
    public readonly float VolumeOverride;
    public readonly bool Preview;
    public readonly int PreviewCount;
    public readonly bool FromRadio;

    public PlayBarkEvent(
        string soundPath,
        NetEntity sourceUid,
        string message,
        float playbackSpeed,
        float pitch,
        bool isObfuscated,
        bool isWhisper,
        float volumeOverride = -1f,
        bool preview = false,
        int previewCount = 1,
        bool fromRadio = false)
    {
        SoundPath = soundPath;
        SourceUid = sourceUid;
        Message = message;
        PlaybackSpeed = playbackSpeed;
        Pitch = pitch;
        IsObfuscated = isObfuscated;
        IsWhisper = isWhisper;
        VolumeOverride = volumeOverride;
        Preview = preview;
        PreviewCount = previewCount;
        FromRadio = fromRadio;
    }
}
