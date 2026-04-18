using System.Linq;
using Content.Shared.DeviceLinking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Network;

namespace Content.Shared._CCM.FpvDrone;

public abstract class SharedFpvDroneLaptopSystem : EntitySystem
{
    [Dependency] protected readonly INetManager Net = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem Ui = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedDeviceLinkSystem DeviceLink = default!;
    [Dependency] protected readonly SharedHandsSystem Hands = default!;

    private const float UpdateInterval = 0.5f;
    private float _updateTimer;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneLaptopComponent, AfterInteractEvent>(OnLaptopAfterInteract);
        SubscribeLocalEvent<FpvDroneLaptopComponent, ComponentShutdown>(OnLaptopShutdown);
        SubscribeLocalEvent<FpvDroneLaptopComponent, ActivatableUIOpenAttemptEvent>(OnLaptopUIOpenAttempt);
        SubscribeLocalEvent<FpvDroneLaptopComponent, EntParentChangedMessage>(OnLaptopParentChanged);

        SubscribeLocalEvent<FpvDroneLaptopLinkedComponent, ComponentShutdown>(OnDroneLinkedShutdown);

        SubscribeLocalEvent<FpvDroneLaptopComponent, FpvDroneLaptopUnlinkBuiMsg>(OnUnlinkMessage);
        SubscribeLocalEvent<FpvDroneLaptopComponent, FpvDroneLaptopUnlinkAllBuiMsg>(OnUnlinkAllMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Net.IsServer)
            return;

        _updateTimer += frameTime;
        if (_updateTimer < UpdateInterval)
            return;

        _updateTimer = 0f;

        UpdateAllOpenUIs();
        CheckLaptopSurfaces();
    }

    private void UpdateAllOpenUIs()
    {
        var query = EntityQueryEnumerator<FpvDroneLaptopComponent>();
        while (query.MoveNext(out var uid, out var laptop))
        {
            if (Ui.IsUiOpen(uid, FpvDroneLaptopUiKey.Key))
                UpdateUI((uid, laptop));
        }
    }

    private void CheckLaptopSurfaces()
    {
        var query = EntityQueryEnumerator<FpvDroneLaptopComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            var parent = xform.ParentUid;
            if (!HasComp<PlaceableSurfaceComponent>(parent))
                continue;

            if (!TryComp(parent, out TransformComponent? parentXform) || !parentXform.Anchored)
            {
                TransformSystem.AttachToGridOrMap(uid);
            }
        }
    }

    protected virtual void UpdateUI(Entity<FpvDroneLaptopComponent> laptop)
    {
    }

    private void OnLaptopAfterInteract(Entity<FpvDroneLaptopComponent> laptop, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!HasComp<PlaceableSurfaceComponent>(args.Target.Value))
            return;

        args.Handled = true;

        if (!Hands.TryDrop(args.User, laptop.Owner, checkActionBlocker: false))
            return;

        if (Net.IsServer)
            PlaceLaptopOnSurface(laptop, args.Target.Value, args.User);
    }

    private void OnLaptopUIOpenAttempt(Entity<FpvDroneLaptopComponent> laptop, ref ActivatableUIOpenAttemptEvent args)
    {
        var parent = Transform(laptop).ParentUid;
        if (!HasComp<PlaceableSurfaceComponent>(parent))
        {
            Popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-place-first"), laptop, args.User);
            args.Cancel();
            return;
        }

        if (!laptop.Comp.IsOpen)
        {
            laptop.Comp.IsOpen = true;
            SetPowered(laptop, true);
            UpdateLaptopVisuals(laptop);
            Dirty(laptop);
        }
    }

    private void OnLaptopShutdown(Entity<FpvDroneLaptopComponent> laptop, ref ComponentShutdown args)
    {
        UnlinkAllDrones(laptop);
    }

    private void OnDroneLinkedShutdown(Entity<FpvDroneLaptopLinkedComponent> drone, ref ComponentShutdown args)
    {
        if (drone.Comp.LinkedLaptop == null)
            return;

        if (!TryComp(drone.Comp.LinkedLaptop.Value, out FpvDroneLaptopComponent? laptop))
            return;

        UnlinkDrone((drone.Comp.LinkedLaptop.Value, laptop), drone.Owner);
    }

    private void OnUnlinkMessage(Entity<FpvDroneLaptopComponent> laptop, ref FpvDroneLaptopUnlinkBuiMsg args)
    {
        if (!Net.IsServer)
            return;

        if (!TryGetEntity(args.Drone, out var droneEnt))
            return;

        UnlinkDrone(laptop, droneEnt.Value);

        Popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-unlinked"), laptop, args.Actor);

        UpdateUI(laptop);
    }

    private void OnUnlinkAllMessage(Entity<FpvDroneLaptopComponent> laptop, ref FpvDroneLaptopUnlinkAllBuiMsg args)
    {
        if (!Net.IsServer)
            return;

        UnlinkAllDrones(laptop);

        Popup.PopupEntity(Loc.GetString("cm-fpv-drone-laptop-unlinked"), laptop, args.Actor);

        UpdateUI(laptop);
    }

    private void OnLaptopParentChanged(Entity<FpvDroneLaptopComponent> laptop, ref EntParentChangedMessage args)
    {
        var parent = Transform(laptop).ParentUid;
        var onSurface = HasComp<PlaceableSurfaceComponent>(parent);
        var parentAnchored = TryComp(parent, out TransformComponent? parentXform) && parentXform.Anchored;

        laptop.Comp.IsOpen = onSurface && parentAnchored;
        SetPowered(laptop, onSurface && parentAnchored);
        UpdateLaptopVisuals(laptop);
        Dirty(laptop);

        if (Net.IsServer && (!onSurface || !parentAnchored))
            Ui.CloseUi(laptop.Owner, FpvDroneLaptopUiKey.Key);
    }

    protected void PlaceLaptopOnSurface(Entity<FpvDroneLaptopComponent> laptop, EntityUid surface, EntityUid user)
    {
        var surfaceXform = Transform(surface);

        TransformSystem.SetCoordinates(laptop.Owner, surfaceXform.Coordinates);
        TransformSystem.SetParent(laptop.Owner, surface);

        laptop.Comp.IsOpen = true;
        SetPowered(laptop, true);
        UpdateLaptopVisuals(laptop);
        Dirty(laptop);
    }

    protected bool ValidateLaptopForLinking(Entity<FpvDroneLaptopComponent> laptop, EntityUid drone, EntityUid user)
    {
        if (!laptop.Comp.IsOpen)
        {
            Popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-place-first"), laptop, user);
            return false;
        }

        if (GetLinkedDrones(laptop).Count >= laptop.Comp.MaxLinkedDrones)
        {
            Popup.PopupClient(Loc.GetString("cm-fpv-drone-laptop-capacity"), laptop, user);
            return false;
        }

        return true;
    }

    protected bool IsDroneAlreadyLinked(EntityUid drone)
    {
        return TryComp<FpvDroneLaptopLinkedComponent>(drone, out var linked) && linked.LinkedLaptop != null;
    }

    protected virtual void UnlinkDrone(Entity<FpvDroneLaptopComponent> laptop, EntityUid drone)
    {
        if (!laptop.Comp.LinkedDrones.Remove(drone))
            return;

        ClearWatchersForDrone(drone);

        if (TryComp<DeviceLinkSinkComponent>(drone, out var sink))
            DeviceLink.RemoveAllFromSink(drone, sink);

        RemComp<FpvDroneLaptopLinkedComponent>(drone);

        if (TryComp<FpvDroneComponent>(drone, out var fpvDrone))
        {
            fpvDrone.Control = default;
            Dirty(drone, fpvDrone);
        }

        if (laptop.Comp.LinkedDrones.Count == 0)
            SetPowered(laptop, false);

        Dirty(laptop);
    }

    protected void UnlinkAllDrones(Entity<FpvDroneLaptopComponent> laptop)
    {
        var drones = GetLinkedDrones(laptop).ToList();
        foreach (var drone in drones)
        {
            UnlinkDrone(laptop, drone);
        }
    }

    protected virtual void ClearWatchersForDrone(EntityUid drone)
    {
    }

    public void SetPowered(Entity<FpvDroneLaptopComponent> laptop, bool powered)
    {
        laptop.Comp.IsPowered = powered;
        UpdateLaptopVisuals(laptop);
        Dirty(laptop);
    }

    private void UpdateLaptopVisuals(Entity<FpvDroneLaptopComponent> laptop)
    {
        var state = FpvDroneLaptopState.Closed;

        if (laptop.Comp.IsOpen)
            state = laptop.Comp.IsPowered ? FpvDroneLaptopState.Active : FpvDroneLaptopState.Open;

        Appearance.SetData(laptop, FpvDroneLaptopVisuals.State, state);
    }

    protected virtual List<EntityUid> GetLinkedDrones(Entity<FpvDroneLaptopComponent> laptop)
    {
        var linked = new List<EntityUid>();

        if (TryComp<DeviceLinkSourceComponent>(laptop, out var source))
        {
            foreach (var sink in source.LinkedPorts.Keys)
            {
                if (HasComp<FpvDroneComponent>(sink))
                    linked.Add(sink);
            }

            laptop.Comp.LinkedDrones = linked.ToHashSet();
            return linked;
        }

        linked.AddRange(laptop.Comp.LinkedDrones);
        return linked;
    }
}
