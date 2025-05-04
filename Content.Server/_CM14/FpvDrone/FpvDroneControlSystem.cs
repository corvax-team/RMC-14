using Content.Server.Mind;
using Content.Shared.Coordinates;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Inventory;
using Robust.Shared.Map;

namespace Content.Server.FpvDrone;

public sealed class FpvDroneControlSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FpvDroneControlComponent, InteractHandEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, FpvDroneControlComponent component, InteractHandEvent args)
    {
        if (!args.User.IsValid())
            return;

        if (component.Used)
        {
            _popup.PopupClient("Console already used!", uid, args.User);
            return;
        }

        if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
            return;

        
        if (!_inventory.TryGetSlotEntity(args.User, "eyes", out var eyesUid) || 
            !TryComp<FpvDroneGogglesComponent>(eyesUid, out _))
        {
            _popup.PopupClient("You need to wear FPV Goggles to use the console!", uid, args.User);
            return;
        }

        
        component.Used = true;

        var observer = Spawn(component.ObserverPrototypeId, Transform(uid).Coordinates);
        component.Observer = observer;
        component.Pilot = args.User;

        var obsComp = EnsureComp<FpvDroneObserverComponent>(observer);
        obsComp.Control = uid;

        _mind.TransferTo(mindId, observer, mind: mind);
    }
}