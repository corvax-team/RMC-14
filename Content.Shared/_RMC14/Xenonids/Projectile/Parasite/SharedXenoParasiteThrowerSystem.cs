using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Rest;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Ghost.Roles;
using Content.Shared.Item;
using Content.Shared.UserInterface;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Hands;
using Content.Shared.Verbs;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

public abstract partial class SharedXenoParasiteThrowerSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoParasiteThrowerComponent, ExaminedEvent>(OnParasiteThrowerExamine);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, XenoChangeParasiteReserveMessage>(OnParasiteReserveChange);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, CCMXenoChangeRoyalParasiteReserveMessage>(OnRoyalParasiteReserveChange);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, XenoReserveParasiteActionEvent>(OnSetReserve);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, ThrowItemAttemptEvent>(OnThrowAttempt);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, ThrowAttemptEvent>(OnPreThrowAttempt);
    }

    private void OnParasiteThrowerExamine(Entity<XenoParasiteThrowerComponent> thrower, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(XenoParasiteThrowerComponent)))
        {
            var totalParasites = thrower.Comp.CurParasites + thrower.Comp.CurParasitesInHands;
            var totalRoyalParasites = thrower.Comp.CurRoyalParasites + thrower.Comp.CurRoyalParasitesInHands;

            if (HasComp<XenoComponent>(args.Examiner))
            {
                args.PushMarkup(Loc.GetString("rmc-xeno-throw-parasite-current", ("cur_paras", totalParasites), ("max_paras", thrower.Comp.MaxParasites)));
                args.PushMarkup(Loc.GetString("rmc-xeno-throw-royal-parasite-current", ("cur_royals", totalRoyalParasites), ("max_royals", thrower.Comp.MaxRoyalParasites)));
            }

            if (HasComp<GhostComponent>(args.Examiner))
            {
                var availableRegular = Math.Max(0, totalParasites - thrower.Comp.ReservedParasites);
                var availableRoyal = Math.Max(0, totalRoyalParasites - thrower.Comp.ReservedRoyalParasites);
                var availableRoles = availableRegular + availableRoyal;

                if (availableRoles > 0)
                {
                    args.PushMarkup(Loc.GetString("rmc-xeno-parasite-ghost-roles-available", ("count", availableRoles)));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("rmc-xeno-parasite-ghost-carrier-none", ("xeno", thrower)));
                }
            }
        }
    }

    private void OnParasiteReserveChange(Entity<XenoParasiteThrowerComponent> thrower, ref XenoChangeParasiteReserveMessage args)
    {
        var inHandParasites = CountParasitesInHand(thrower.Owner, false);
        var totalParasites = Math.Max(0, thrower.Comp.CurParasites + inHandParasites);
        var newVal = Math.Clamp(args.NewReserve, 0, totalParasites);
        thrower.Comp.ReservedParasites = newVal;
        Dirty(thrower);
    }

    private void OnRoyalParasiteReserveChange(Entity<XenoParasiteThrowerComponent> thrower, ref CCMXenoChangeRoyalParasiteReserveMessage args)
    {
        var inHandRoyalParasites = CountParasitesInHand(thrower.Owner, true);
        var totalRoyalParasites = Math.Max(0, thrower.Comp.CurRoyalParasites + inHandRoyalParasites);
        var newVal = Math.Clamp(args.NewRoyalReserve, 0, totalRoyalParasites);
        thrower.Comp.ReservedRoyalParasites = newVal;
        Dirty(thrower);
    }

    private int CountParasitesInHand(EntityUid owner, bool countRoyals)
    {
        var count = 0;
        var heldItems = _hands.EnumerateHeld(owner);

        foreach (var item in heldItems)
        {
            if (TryComp<XenoParasiteComponent>(item, out _))
            {
                var isRoyal = HasComp<CCMRoyalParasiteComponent>(item);
                if (isRoyal == countRoyals)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void OnSetReserve(Entity<XenoParasiteThrowerComponent> xeno, ref XenoReserveParasiteActionEvent args)
    {
        if (args.Handled)
        {
            return;
        }

        _ui.OpenUi(xeno.Owner, XenoReserveParasiteChangeUI.Key, xeno);

        args.Handled = true;
    }

    private void OnPreThrowAttempt(Entity<XenoParasiteThrowerComponent> thrower, ref ThrowAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!HasComp<XenoParasiteComponent>(args.ItemUid))
            return;

        thrower.Comp.LastThrownParasite = args.ItemUid;
    }

    private void OnThrowAttempt(Entity<XenoParasiteThrowerComponent> thrower, ref ThrowItemAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var itemUid = thrower.Comp.LastThrownParasite;
        if (itemUid == null)
            return;

        var isRoyal = HasComp<CCMRoyalParasiteComponent>(itemUid.Value);

        if (isRoyal)
        {
            var totalRoyalParasites = thrower.Comp.CurRoyalParasites + thrower.Comp.CurRoyalParasitesInHands;
            if (totalRoyalParasites <= 0)
            {
                args.Cancelled = true;
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-royal-parasites"), thrower.Owner, args.User);
                return;
            }
        }
        else
        {
            var totalRegularParasites = thrower.Comp.CurParasites + thrower.Comp.CurParasitesInHands;
            if (totalRegularParasites <= 0)
            {
                args.Cancelled = true;
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-parasites"), thrower.Owner, args.User);
                return;
            }
        }
    }
}
