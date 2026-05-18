using Content.Shared._CE.ZLevels.Core.Components;
using JetBrains.Annotations;
using Robust.Shared.Physics;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    [PublicAPI]
    public void WakeBody(Entity<CEZPhysicsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!ZLevelsEnabled || !ShouldTrackClientPhysics)
        {
            entity.Comp.Sleeping = true;
            entity.Comp.SleepTimer = 0f;
            RemoveActiveBody(entity.Owner);
            return;
        }

        if (_activeBodyIndices.ContainsKey(entity.Owner))
            return;

        entity.Comp.Sleeping = false;
        entity.Comp.SleepTimer = 0f;
        _activeBodyIndices[entity.Owner] = _activeBodies.Count;
        _activeBodies.Add(entity.Owner);
    }

    [PublicAPI]
    public void SleepBody(Entity<CEZPhysicsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.Sleeping = true;
        entity.Comp.SleepTimer = 0f;
        RemoveActiveBody(entity.Owner);
    }

    [PublicAPI]
    public void RefreshBody(Entity<CEZPhysicsComponent> entity)
    {
        if (TerminatingOrDeleted(entity))
        {
            SleepBody((entity, entity.Comp));
            return;
        }

        if (!ShouldTrackClientPhysics)
        {
            SleepBody((entity, entity.Comp));
            return;
        }

        var transform = Transform(entity);
        var parent = transform.ParentUid;
        var mapUid = transform.MapUid;

        if (!ZLevelsEnabled ||
            mapUid is not { } ||
            !_zMapQuery.HasComp(mapUid.Value) ||
            parent != mapUid ||
            transform.Anchored ||
            _physicsQuery.TryComp(entity.Owner, out var physics) &&
            physics.BodyType == BodyType.Static)
        {
            SleepBody((entity, entity.Comp));
            return;
        }

        WakeBody((entity, entity.Comp));
    }

    private void ClearActiveBodies()
    {
        _activeBodies.Clear();
        _activeBodyIndices.Clear();
    }

    private void RemoveActiveBody(EntityUid uid)
    {
        if (!_activeBodyIndices.Remove(uid, out var index))
            return;

        var lastIndex = _activeBodies.Count - 1;
        if (index != lastIndex)
        {
            var lastUid = _activeBodies[lastIndex];
            _activeBodies[index] = lastUid;
            _activeBodyIndices[lastUid] = index;
        }

        _activeBodies.RemoveAt(lastIndex);
    }

    private void RebuildBodyTracking()
    {
        ClearActiveBodies();
        ClearDirtyMovement();

        if (!ZLevelsEnabled)
            return;

        var query = EntityQueryEnumerator<CEZPhysicsComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var zPhysics, out var xform))
        {
            if (xform.MapUid is { } mapUid &&
                _zMapQuery.TryComp(mapUid, out var zMap) &&
                zPhysics.CurrentZLevel != zMap.Depth)
            {
                zPhysics.CurrentZLevel = zMap.Depth;
                DirtyField(uid, zPhysics, nameof(CEZPhysicsComponent.CurrentZLevel));
            }

            DirtyMovement((uid, zPhysics));
            RefreshBody((uid, zPhysics));
        }
    }
}
