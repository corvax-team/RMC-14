/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    private readonly List<EntityUid> _activeBodies = new();
    private readonly Dictionary<EntityUid, int> _activeBodyIndices = new();
    public IReadOnlyList<EntityUid> ActiveBodies => _activeBodies;

    private void InitializeActivation()
    {
        SubscribeLocalEvent<CEZPhysicsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CEZPhysicsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CEZPhysicsComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
        SubscribeLocalEvent<CEZPhysicsComponent, PhysicsBodyTypeChangedEvent>(OnPhysicsBodyTypeChanged);
        SubscribeLocalEvent<CEZPhysicsComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnMapInit(Entity<CEZPhysicsComponent> entity, ref MapInitEvent args)
    {
        RefreshBody(entity);

        var mapUid = Transform(entity).MapUid;
        if (!_zMapQuery.TryComp(mapUid, out var zLevel))
            return;

        if (entity.Comp.CurrentZLevel == zLevel.Depth)
            return;

        entity.Comp.CurrentZLevel = zLevel.Depth;
        DirtyField(entity.Owner, entity.Comp, nameof(CEZPhysicsComponent.CurrentZLevel));
    }

    private void OnShutdown(Entity<CEZPhysicsComponent> entity, ref ComponentShutdown args)
    {
        SleepBody((entity.Owner, entity.Comp));
    }

    private void OnAnchorStateChanged(Entity<CEZPhysicsComponent> entity, ref AnchorStateChangedEvent args)
    {
        RefreshBody(entity);
    }

    private void OnPhysicsBodyTypeChanged(Entity<CEZPhysicsComponent> entity, ref PhysicsBodyTypeChangedEvent args)
    {
        RefreshBody(entity);
    }

    protected virtual void OnParentChanged(Entity<CEZPhysicsComponent> entity, ref EntParentChangedMessage args)
    {
        RefreshBody(entity);

        if (ZPhysicsQuery.TryComp(args.OldParent, out var oldParentZPhys))
            SetZPosition((entity.Owner, entity.Comp), oldParentZPhys.LocalPosition);
    }

    protected void RefreshZPhysicsOnMap(Entity<CEZLevelMapComponent> map)
    {
        var query = EntityQueryEnumerator<CEZPhysicsComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var zPhysics, out var xform))
        {
            if (xform.MapUid != map.Owner)
                continue;

            zPhysics.CurrentZLevel = map.Comp.Depth;
            DirtyField(uid, zPhysics, nameof(CEZPhysicsComponent.CurrentZLevel));
            DirtyMovement((uid, zPhysics));
            RefreshBody((uid, zPhysics));
        }
    }
}
