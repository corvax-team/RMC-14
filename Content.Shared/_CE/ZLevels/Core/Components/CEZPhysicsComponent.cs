/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Numerics;
using Robust.Shared.GameStates;
using Content.Shared._CE.ZLevels.Core.EntitySystems;

namespace Content.Shared._CE.ZLevels.Core.Components;

/// <summary>
/// Allows an entity to move up and down the z-levels by gravity or jumping
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true),
 Access(typeof(CESharedZLevelsSystem))]
public sealed partial class CEZPhysicsComponent : Component
{
    /// <summary>
    /// The current speed of movement between z-levels.
    /// If greater than 0, the entity moves upward. If less than 0, the entity moves downward.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Velocity;

    /// <summary>
    /// The current height of the entity within the current Z-level.
    /// Takes values from 0 to 1. If the value rises above 1, the entity moves up to the next level and the value is normalized.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LocalPosition;

    /// Optimization Caches

    /// <summary>
    /// Cached value of the current z-level map height
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentZLevel;

    /// <summary>
    /// Cached tile position for optimization - not networked as it's client/server specific
    /// </summary>
    [ViewVariables]
    public Vector2i? CachedTile;

    // Physics

    [DataField, AutoNetworkedField]
    public float Bounciness = 0.3f;

    [DataField, AutoNetworkedField]
    public float GravityMultiplier = 1f;

    [DataField, AutoNetworkedField]
    public bool Fallable = true;

    // Visuals

    /// <summary>
    /// Used only by the client.
    /// Blocks the rotation of an object if it has <see cref="LocalPosition"/> > 0,
    /// and saves the original NoRot value in SpriteComponent here so that it can be restored in the future.
    /// </summary>
    [DataField]
    public bool NoRotDefault;

    /// <summary>
    /// The original DrawDepth of the object is automatically saved here. Increases by 1 when the creature has <see cref="LocalPosition"/> > 0
    /// </summary>
    [DataField]
    public int DrawDepthDefault;

    /// <summary>
    /// When the mapinit entity is created, its initial Sprite Offset value is written here in order to apply an offset based on the Z position relative to this value.
    /// </summary>
    [DataField]
    public Vector2 SpriteOffsetDefault = Vector2.Zero;

    /// <summary>
    /// automatically rises if the current localPosition is lower than the height. Enabled by default, but for ghosts, for example, there is no point in climbing stairs
    /// </summary>
    [DataField]
    public bool AutoStep = true;

    #region Gravity

    [DataField]
    public bool VelocityGravity = true;

    [DataField]
    public bool VelocityRaiseEvent;

    #endregion

    #region Cache

    /// <summary>
    /// Cached value of the current distance to the ground in the current z-level. Updates only on MoveEvent and when tiles below change.
    /// </summary>
    [ViewVariables]
    public float CachedGroundHeight;

    /// <summary>
    /// Cached value of whether the entity is currently on sticky ground (ladders).
    /// </summary>
    [ViewVariables]
    public bool CachedStickyGround;

    /// <summary>
    /// Cached value of whether there is a solid tile directly above this entity.
    /// </summary>
    [ViewVariables]
    public bool CachedHasTileAbove;

    /// <summary>
    /// Cached map UID for ground height calculation validation
    /// </summary>
    [ViewVariables]
    public EntityUid? CachedMapUid;

    /// <summary>
    /// Runtime-only optimization flag. Sleeping entities skip expensive z-physics integration
    /// until movement, cache invalidation or explicit velocity changes wake them up.
    /// </summary>
    [ViewVariables]
    public bool Sleeping;

    /// <summary>
    /// How long the body has stayed nearly motionless on the ground.
    /// </summary>
    [DataField]
    public float SleepTimer;

    /// <summary>
    /// Velocity threshold below which the body may start falling asleep.
    /// </summary>
    [DataField]
    public float SleepThreshold = 0.3f;

    /// <summary>
    /// Time in seconds a nearly motionless body must remain idle before sleeping.
    /// </summary>
    [DataField]
    public float TimeToSleep = 2f;

    /// <summary>
    /// Prevents sticky stairs from immediately reversing a Z transition on the same tile.
    /// Cleared as soon as the entity leaves that tile or map.
    /// </summary>
    [ViewVariables]
    public Vector2i? SuppressedStairTransitionTile;

    [ViewVariables]
    public EntityUid? SuppressedStairTransitionMap;

    [ViewVariables]
    public int SuppressedStairTransitionOffset;

    #endregion
}
