using System.Numerics;
using Content.Shared._CCM.Silencer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Silencer;

public sealed class CCMSonicPulseVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public readonly record struct Wave(MapCoordinates Origin, Vector2 Direction, float Range, Angle Cone, TimeSpan Start, float Duration);

    public const int MaxWaves = 8;

    private readonly List<Wave> _waves = new();
    public IReadOnlyList<Wave> Waves => _waves;

    private TimeSpan? _lastPredicted;

    private CCMSonicPulseOverlay _pulseOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMSonicPulsePredictedWaveEvent>(OnPredictedWave);
        SubscribeNetworkEvent<CCMSonicPulseWaveEvent>(OnNetworkWave);

        _pulseOverlay = new CCMSonicPulseOverlay();
        _overlay.AddOverlay(_pulseOverlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay(_pulseOverlay);
        _waves.Clear();
    }

    private void OnPredictedWave(ref CCMSonicPulsePredictedWaveEvent ev)
    {
        if (ev.Shooter != _player.LocalEntity)
            return;

        _lastPredicted = _timing.RealTime;
        AddWave(ev.Origin, ev.Direction, ev.Range, ev.Cone, ev.Duration);
    }

    private void OnNetworkWave(CCMSonicPulseWaveEvent ev)
    {
        // Skip the server copy of a wave the local shooter already predicted.
        if (GetEntity(ev.Shooter) == _player.LocalEntity &&
            _lastPredicted is { } last &&
            _timing.RealTime - last < TimeSpan.FromSeconds(1))
        {
            _lastPredicted = null;
            return;
        }

        var coords = GetCoordinates(ev.Origin);
        if (!coords.IsValid(EntityManager))
            return;

        AddWave(_transform.ToMapCoordinates(coords), ev.Direction, ev.Range, ev.Cone, ev.Duration);
    }

    private void AddWave(MapCoordinates origin, Vector2 direction, float range, Angle cone, float duration)
    {
        if (origin.MapId == MapId.Nullspace || direction.LengthSquared() < 0.0001f)
            return;

        if (_waves.Count >= MaxWaves)
            _waves.RemoveAt(0);

        _waves.Add(new Wave(origin, Vector2.Normalize(direction), range, cone, _timing.RealTime, MathF.Max(duration, 0.05f)));
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        _waves.RemoveAll(w => (now - w.Start).TotalSeconds > w.Duration);
    }

    public float GetProgress(in Wave wave)
    {
        var t = (float) ((_timing.RealTime - wave.Start).TotalSeconds / wave.Duration);
        return Math.Clamp(t, 0f, 1f);
    }
}
