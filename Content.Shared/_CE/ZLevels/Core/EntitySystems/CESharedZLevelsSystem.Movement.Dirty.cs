using Content.Shared._CE.ZLevels.Core.Components;
using JetBrains.Annotations;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    private readonly HashSet<EntityUid> _dirtyMovementBodies = new();
    private readonly List<EntityUid> _dirtyMovementSnapshot = new();

    [PublicAPI]
    public void DirtyMovement(Entity<CEZPhysicsComponent?> entity)
    {
        if (!ZLevelsEnabled || !ShouldTrackClientPhysics)
            return;

        _dirtyMovementBodies.Add(entity.Owner);
    }

    private void UpdateDirtyMovement()
    {
        if (!ZLevelsEnabled)
        {
            _dirtyMovementBodies.Clear();
            _dirtyMovementSnapshot.Clear();
            return;
        }

        _dirtyMovementSnapshot.Clear();
        _dirtyMovementSnapshot.AddRange(_dirtyMovementBodies);
        _dirtyMovementBodies.Clear();

        foreach (var uid in _dirtyMovementSnapshot)
        {
            if (!ZPhysicsQuery.TryComp(uid, out var component))
                continue;

            var entity = (uid, component);
            RequestCacheMovement(entity);
            RefreshBody(entity);
        }

        _dirtyMovementSnapshot.Clear();
    }

    private void ClearDirtyMovement()
    {
        _dirtyMovementBodies.Clear();
        _dirtyMovementSnapshot.Clear();
    }
}
