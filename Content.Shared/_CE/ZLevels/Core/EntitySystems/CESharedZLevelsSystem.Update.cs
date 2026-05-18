using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    public int UpdateCalls { get; private set; }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateCalls = 0;

        if (!ZLevelsEnabled)
        {
            _accumulatedTime = TimeSpan.Zero;
            ClearActiveBodies();
            ClearDirtyMovement();
            return;
        }

        if (_net.IsClient && !_clientSimulation)
        {
            _accumulatedTime = TimeSpan.Zero;
            ClearActiveBodies();
            ClearDirtyMovement();
            return;
        }

        if (_zMapCount == 0)
        {
            _accumulatedTime = TimeSpan.Zero;
            return;
        }

        GuardClientBodyTracking();

        // Sleeping bodies re-enter z-physics through DirtyMovement/RefreshBody.
        // Process that queue even when there are currently no active bodies,
        // otherwise a fully sleeping population never wakes back up for stairs/ladders.
        UpdateDirtyMovement();

        if (_activeBodies.Count == 0)
        {
            _accumulatedTime = TimeSpan.Zero;
            return;
        }

        _accumulatedTime += TimeSpan.FromSeconds(frameTime);

        var steps = 0;
        while (_accumulatedTime >= _fixedTimestep && steps < MaxStepsPerFrame)
        {
            UpdateZPhysics((float) _fixedTimestep.TotalSeconds);
            _accumulatedTime -= _fixedTimestep;
            steps++;
        }
    }

    private void GuardClientBodyTracking()
    {
        if (!_net.IsClient || !_clientSimulation)
            return;

        var dirtyCount = _dirtyMovementBodies.Count;
        var activeCount = _activeBodies.Count;
        if (dirtyCount < ClientSoftBodyTrackingLimit &&
            activeCount < ClientSoftBodyTrackingLimit)
        {
            return;
        }

        if (_timing.CurTime < _nextClientBodyTrackingRecovery)
            return;

        _nextClientBodyTrackingRecovery = _timing.CurTime + TimeSpan.FromSeconds(ClientBodyTrackingRecoveryCooldown);

        if (dirtyCount >= ClientHardBodyTrackingLimit ||
            activeCount >= ClientHardBodyTrackingLimit)
        {
            Log.Warning($"Client z-level tracking overflow detected (active={activeCount}, dirty={dirtyCount}). Clearing client body tracking.");
            ClearActiveBodies();
            ClearDirtyMovement();
            return;
        }

        Log.Warning($"Client z-level tracking spike detected (active={activeCount}, dirty={dirtyCount}). Rebuilding client body tracking.");
        RebuildBodyTracking();
    }

    private void UpdateZPhysics(float frameTime)
    {
        UpdateDirtyMovement();

        for (var i = _activeBodies.Count - 1; i >= 0; i--)
        {
            var uid = _activeBodies[i];

            if (!ZPhysicsQuery.TryComp(uid, out var zPhysicsComponent) ||
                !_transformQuery.TryComp(uid, out var xform) ||
                !_physicsQuery.TryComp(uid, out var physics))
            {
                RemoveActiveBody(uid);
                continue;
            }

            if (xform.MapUid == null || xform.MapUid == EntityUid.Invalid || !_zMapQuery.HasComp(xform.MapUid.Value))
            {
                RemoveActiveBody(uid);
                continue;
            }

            ProcessZPhysics((uid, zPhysicsComponent, physics), frameTime);
        }
    }

    private void ProcessZPhysics(Entity<CEZPhysicsComponent, PhysicsComponent> entity, float frameTime)
    {
        UpdateCalls++;

        var zPhysicsComponent = entity.Comp1;
        var physicsComponent = entity.Comp2;

        var oldVelocity = zPhysicsComponent.Velocity;
        var oldHeight = zPhysicsComponent.LocalPosition;

        if (physicsComponent.BodyStatus == BodyStatus.OnGround)
        {
            if (zPhysicsComponent.VelocityGravity)
                zPhysicsComponent.Velocity -= ZGravityForce * zPhysicsComponent.GravityMultiplier * frameTime;

            if (zPhysicsComponent.VelocityRaiseEvent)
            {
                var velocityEvent = new CEGetZVelocityEvent((entity.Owner, zPhysicsComponent));
                RaiseLocalEvent(entity.Owner, ref velocityEvent);
                zPhysicsComponent.Velocity += velocityEvent.VelocityDelta * frameTime;
            }
        }

        zPhysicsComponent.LocalPosition += zPhysicsComponent.Velocity * frameTime;

        var distanceToGround = zPhysicsComponent.LocalPosition - zPhysicsComponent.CachedGroundHeight;

        if (zPhysicsComponent.AutoStep && distanceToGround < 0)
            zPhysicsComponent.LocalPosition -= distanceToGround;

        if (zPhysicsComponent.CachedStickyGround)
            zPhysicsComponent.LocalPosition -= distanceToGround;

        if (zPhysicsComponent is { Velocity: < 0, Fallable: true })
        {
            if (distanceToGround <= 0.05f)
            {
                var nearHighGround = HasNearbyHighGroundForFallImpact(entity.Owner);
                if (float.Abs(zPhysicsComponent.Velocity) >= ImpactVelocityLimit && !nearHighGround)
                {
                    var hitEv = new CEZLevelHitEvent(-zPhysicsComponent.Velocity);
                    RaiseLocalEvent(entity.Owner, ref hitEv);

                    var land = new LandEvent(null, true);
                    RaiseLocalEvent(entity.Owner, ref land);
                }

                if (float.Abs(zPhysicsComponent.Velocity) < zPhysicsComponent.SleepThreshold)
                {
                    zPhysicsComponent.Velocity = 0f;
                    zPhysicsComponent.LocalPosition = zPhysicsComponent.CachedGroundHeight;
                }
                else
                {
                    zPhysicsComponent.Velocity = -zPhysicsComponent.Velocity * zPhysicsComponent.Bounciness;
                }
            }
        }

        if (zPhysicsComponent.LocalPosition < 0)
        {
            var stairTransition = zPhysicsComponent.CachedStickyGround ||
                                  IsStairTransitionSuppressed((entity.Owner, zPhysicsComponent), -1);

            if (stairTransition && IsStairTransitionSuppressed((entity.Owner, zPhysicsComponent), -1))
            {
                zPhysicsComponent.LocalPosition = 0.05f;
                zPhysicsComponent.Velocity = 0f;
            }
            else if (TryMoveDownOrChasm(entity.Owner))
            {
                zPhysicsComponent.LocalPosition = stairTransition
                    ? GetPostStairTransitionLocalPosition(zPhysicsComponent, 1)
                    : zPhysicsComponent.LocalPosition + 1;

                if (stairTransition)
                {
                    zPhysicsComponent.Velocity = 0f;
                    SuppressReverseStairTransition((entity.Owner, zPhysicsComponent), 1);
                }

                if (!zPhysicsComponent.CachedStickyGround &&
                    zPhysicsComponent.Fallable &&
                    !HasNearbyHighGroundForFallImpact(entity.Owner))
                {
                    var fallEv = new CEZLevelFallMapEvent();
                    RaiseLocalEvent(entity.Owner, ref fallEv);
                }
            }
        }

        if (zPhysicsComponent.LocalPosition >= 1)
        {
            if (HasTileAbove(entity.Owner))
            {
                if (float.Abs(zPhysicsComponent.Velocity) >= ImpactVelocityLimit &&
                    !HasNearbyHighGroundForFallImpact(entity.Owner))
                {
                    var hitEv = new CEZLevelHitEvent(zPhysicsComponent.Velocity);
                    RaiseLocalEvent(entity.Owner, ref hitEv);

                    var land = new LandEvent(null, true);
                    RaiseLocalEvent(entity.Owner, ref land);
                }

                zPhysicsComponent.LocalPosition = 1;
                zPhysicsComponent.Velocity = -zPhysicsComponent.Velocity * zPhysicsComponent.Bounciness;
            }
            else
            {
                var stairTransition = zPhysicsComponent.CachedStickyGround ||
                                      zPhysicsComponent.CachedGroundHeight >= 1f;

                if (stairTransition && IsStairTransitionSuppressed((entity.Owner, zPhysicsComponent), 1))
                {
                    zPhysicsComponent.LocalPosition = 0.95f;
                    zPhysicsComponent.Velocity = 0f;
                }
                else if (TryMoveUp(entity.Owner))
                {
                    zPhysicsComponent.LocalPosition = stairTransition
                        ? GetPostStairTransitionLocalPosition(zPhysicsComponent, -1)
                        : zPhysicsComponent.LocalPosition - 1;

                    if (stairTransition)
                    {
                        zPhysicsComponent.Velocity = 0f;
                        SuppressReverseStairTransition((entity.Owner, zPhysicsComponent), -1);
                    }
                }
            }
        }

        if (float.Abs(zPhysicsComponent.Velocity) > ZVelocityLimit)
            zPhysicsComponent.Velocity = float.Sign(zPhysicsComponent.Velocity) * ZVelocityLimit;

        if (float.Abs(oldVelocity - zPhysicsComponent.Velocity) > 0.001f)
            DirtyField(entity.Owner, zPhysicsComponent, nameof(CEZPhysicsComponent.Velocity));

        if (float.Abs(oldHeight - zPhysicsComponent.LocalPosition) > 0.001f)
            DirtyField(entity.Owner, zPhysicsComponent, nameof(CEZPhysicsComponent.LocalPosition));

        SleepUpdate((entity.Owner, zPhysicsComponent), frameTime);
    }

    private void SleepUpdate(Entity<CEZPhysicsComponent> entity, float frameTime)
    {
        var distanceToGround = entity.Comp.LocalPosition - entity.Comp.CachedGroundHeight;
        var almostStopped = float.Abs(entity.Comp.Velocity) < entity.Comp.SleepThreshold &&
                            float.Abs(distanceToGround) <= 0.01f;

        if (!almostStopped)
        {
            entity.Comp.SleepTimer = 0f;
            return;
        }

        entity.Comp.SleepTimer += frameTime;
        if (entity.Comp.SleepTimer < entity.Comp.TimeToSleep)
            return;

        SleepBody((entity.Owner, entity.Comp));
    }
}
