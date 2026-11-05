using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Silencer;

[Serializable, NetSerializable]
public sealed class CCMSonicPulseWaveEvent : EntityEventArgs
{
    public NetEntity Shooter;
    public NetCoordinates Origin;
    public Vector2 Direction;
    public float Range;
    public Angle Cone;
    public float Duration;

    public CCMSonicPulseWaveEvent(NetEntity shooter, NetCoordinates origin, Vector2 direction, float range, Angle cone, float duration)
    {
        Shooter = shooter;
        Origin = origin;
        Direction = direction;
        Range = range;
        Cone = cone;
        Duration = duration;
    }
}

[ByRefEvent]
public readonly record struct CCMSonicPulsePredictedWaveEvent(
    EntityUid Shooter,
    MapCoordinates Origin,
    Vector2 Direction,
    float Range,
    Angle Cone,
    float Duration);
