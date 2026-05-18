using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared._CCM.Stats;
using Robust.Shared.Network;

namespace Content.Server.KillTracking;

/// <summary>
/// This is used for entities that track player damage sources and killers.
/// </summary>
[RegisterComponent, Access(typeof(KillTrackingSystem))]
public sealed partial class KillTrackerComponent : Component
{
    /// <summary>
    /// The mobstate that registers as a "kill"
    /// </summary>
    [DataField("killState")]
    public MobState KillState = MobState.Critical;

    /// <summary>
    /// A dictionary of sources and how much damage they've done to this entity over time.
    /// </summary>
    [DataField("lifetimeDamage")]
    public Dictionary<KillSource, FixedPoint2> LifetimeDamage = new();
}

public abstract record KillSource;

/// <summary>
/// A kill source for players
/// </summary>
[DataDefinition, Serializable]
public sealed partial record KillPlayerSource : KillSource
{
    [DataField("playerId")]
    public NetUserId PlayerId;

    [DataField("side")]
    public CCMStatsSide Side;

    public KillPlayerSource(NetUserId playerId, CCMStatsSide side = CCMStatsSide.None)
    {
        PlayerId = playerId;
        Side = side;
    }
}

/// <summary>
/// A kill source for non-player controlled entities
/// </summary>
[DataDefinition, Serializable]
public sealed partial record KillNpcSource : KillSource
{
    [DataField("npcEnt")]
    public EntityUid NpcEnt;

    public KillNpcSource(EntityUid npcEnt)
    {
        NpcEnt = npcEnt;
    }
}

/// <summary>
/// A kill source for kills with no damage origin
/// </summary>
[DataDefinition, Serializable]
public sealed partial record KillEnvironmentSource : KillSource;
