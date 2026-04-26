// CM14 rework: non-RMC edit marker.
using Content.Shared._CCM.Barks;
using Content.Shared.Audio;
using Content.Shared.CCVar;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Barks;

public sealed class BarkSystem : EntitySystem
{
    private const float VoiceRange = 8f;
    private const float WhisperRange = 5f;
    private const float BarkCharsFactor = 0.0675f;
    private const float BarkVolumeMultiplier = 1.4f;
    private const float RadioVolumePenalty = 3.8f;
    private const float SpacePauseMultiplier = 0.35f;

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlayBarkEvent>(OnPlayBark);
    }

    public void RequestPreview(string barkVoiceId, float pitch, float speed)
    {
        if (!_cfg.GetCVar(CCVars.BarksEnabled))
            return;

        RaiseNetworkEvent(new RequestPreviewBarkEvent(barkVoiceId, pitch, speed));
    }

    private void OnPlayBark(PlayBarkEvent ev)
    {
        if (!_cfg.GetCVar(CCVars.BarksEnabled))
            return;

        if (!ev.Preview && string.IsNullOrWhiteSpace(ev.Message))
            return;

        var sourceUid = GetEntity(ev.SourceUid);
        TransformComponent? sourceXform = null;
        var sourceValid = false;
        if (sourceUid != EntityUid.Invalid && EntityManager.EntityExists(sourceUid))
            sourceValid = TryComp(sourceUid, out sourceXform);

        var maxRange = ev.IsWhisper ? WhisperRange : VoiceRange;
        var distanceAttenuation = 0f;

        if (!ev.Preview && !ev.FromRadio)
        {
            if (!sourceValid ||
                sourceXform == null ||
                _player.LocalEntity is not { } local ||
                !TryComp<TransformComponent>(local, out var localXform))
                return;

            if (!sourceXform.Coordinates.TryDelta(EntityManager, _transform, localXform.Coordinates, out var delta))
                return;

            if (delta.LengthSquared() > maxRange * maxRange)
                return;

            var distance = delta.Length();
            var distanceFactor = Math.Clamp(1f - (distance / maxRange), 0.15f, 1f);
            distanceAttenuation = AudioHelpers.SafeGainToVolume(distanceFactor, 1f);
        }

        var speed = Math.Clamp(ev.PlaybackSpeed, 0.7f, 1.4f);
        var pitch = Math.Clamp(ev.Pitch, 0.7f, 1.4f);
        var intervalSeconds = 0.15f / speed;

        var expression = 1f;
        if (sourceValid &&
            TryComp<SpeechSynthesisComponent>(sourceUid, out var synthesis))
        {
            expression = Math.Clamp(synthesis.Expression, 0.25f, 2f);
        }

        var localGain = AudioHelpers.SanitizeGain(_cfg.GetCVar(CCVars.BarksVolume), CCVars.BarksVolume.DefaultValue);
        var volume = AudioHelpers.SafeGainToVolume(localGain, CCVars.BarksVolume.DefaultValue);
        if (ev.VolumeOverride >= 0f)
            volume = AudioHelpers.SanitizeVolume(ev.VolumeOverride, 0f);

        if (ev.IsWhisper)
            volume -= 4f;
        if (ev.FromRadio)
            volume -= RadioVolumePenalty;
        if (ev.IsObfuscated)
            volume -= 4f;
        if (!ev.Preview && !ev.FromRadio)
            volume += distanceAttenuation;

        volume += AudioHelpers.SafeGainToVolume(BarkVolumeMultiplier, BarkVolumeMultiplier);

        var paramsBase = AudioParams.Default
            .WithVolume(AudioHelpers.SanitizeVolume(volume, 0f))
            .WithPitchScale(pitch)
            .WithVariation(0.125f * expression)
            .WithMaxDistance(maxRange);

        var count = 1;
        var message = ev.Message.Trim();
        if (ev.Preview)
        {
            count = Math.Clamp(ev.PreviewCount, 1, 24);
        }
        else
        {
            var length = message.Length;
            var computed = (int) ((length * BarkCharsFactor) / intervalSeconds);
            count = Math.Clamp(Math.Max(1, computed), 1, 24);
        }

        var delays = BuildDelays(message, count, intervalSeconds, ev.Preview);
        for (var i = 0; i < delays.Count; i++)
        {
            var delay = TimeSpan.FromSeconds(delays[i]);
            Timer.Spawn(delay, () => PlaySingle(ev.SoundPath, sourceUid, sourceValid, paramsBase, ev.FromRadio));
        }
    }

    private static List<float> BuildDelays(string message, int count, float intervalSeconds, bool preview)
    {
        var delays = new List<float>(count);
        if (count <= 0)
            return delays;

        if (preview || string.IsNullOrEmpty(message))
        {
            for (var i = 0; i < count; i++)
            {
                delays.Add(intervalSeconds * i);
            }

            return delays;
        }

        var prefixSpaces = new int[message.Length];
        var spaces = 0;
        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace(message[i]))
                spaces++;

            prefixSpaces[i] = spaces;
        }

        var extraPausePerSpace = intervalSeconds * SpacePauseMultiplier;
        for (var i = 0; i < count; i++)
        {
            var baseDelay = intervalSeconds * i;
            var sampledChar = Math.Clamp((int) (((i + 1f) * message.Length) / (count + 1f)), 0, message.Length - 1);
            var extra = prefixSpaces[sampledChar] * extraPausePerSpace;
            delays.Add(baseDelay + extra);
        }

        return delays;
    }

    private void PlaySingle(string soundPath, EntityUid sourceUid, bool sourceValid, AudioParams audioParams, bool forceGlobal)
    {
        var sound = new SoundPathSpecifier(soundPath);

        if (forceGlobal)
        {
            _audio.PlayGlobal(sound, Filter.Local(), false, audioParams);
            return;
        }

        if (sourceValid && EntityManager.EntityExists(sourceUid))
        {
            _audio.PlayPvs(sound, sourceUid, audioParams);
            return;
        }

        _audio.PlayGlobal(sound, Filter.Local(), false, audioParams);
    }
}
