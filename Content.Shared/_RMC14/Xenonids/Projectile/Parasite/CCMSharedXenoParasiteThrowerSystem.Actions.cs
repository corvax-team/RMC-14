using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;
using Content.Shared._RMC14.Xenonids.Parasite;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

public abstract partial class SharedXenoParasiteThrowerSystem
{
    protected bool ValidateParasiteThrow(EntityUid uid, XenoParasiteThrowerComponent component, EntityUid? parasiteUid, EntityUid performer, bool isRoyal)
    {
        if (parasiteUid == null)
            return true;

        var isParasiteRoyal = HasComp<CCMRoyalParasiteComponent>(parasiteUid.Value);
        if (isRoyal != isParasiteRoyal)
        {
            var message = isParasiteRoyal
                ? "rmc-xeno-throw-wrong-parasite-type-royal"
                : "rmc-xeno-throw-wrong-parasite-type-regular";
            _popup.PopupEntity(Loc.GetString(message), uid, performer);
            return false;
        }

        if (isRoyal)
        {
            var totalRoyalParasites = component.CurRoyalParasites + component.CurRoyalParasitesInHands;
            if (totalRoyalParasites <= 0)
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-royal-parasites"), uid, performer);
                return false;
            }
        }
        else
        {
            var totalRegularParasites = component.CurParasites + component.CurParasitesInHands;
            if (totalRegularParasites <= 0)
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-parasites"), uid, performer);
                return false;
            }
        }

        return true;
    }

    private void OnThrowParasiteAction(EntityUid uid, XenoParasiteThrowerComponent component, XenoThrowParasiteActionEvent args)
    {
        if (args.Handled)
            return;

        var itemUid = component.LastThrownParasite;
        if (itemUid == null)
            return;

        if (HasComp<CCMRoyalParasiteComponent>(itemUid.Value))
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-wrong-parasite-type-royal"), uid, args.Performer);
            return;
        }

        var totalRegularParasites = component.CurParasites + component.CurParasitesInHands;
        if (totalRegularParasites <= 0)
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-parasites"), uid, args.Performer);
            return;
        }

        args.Handled = true;
    }

    private void OnThrowRoyalParasiteAction(EntityUid uid, XenoParasiteThrowerComponent component, CCMXenoThrowRoyalParasiteActionEvent args)
    {
        if (args.Handled)
            return;

        var itemUid = component.LastThrownParasite;
        if (itemUid == null)
            return;

        if (!HasComp<CCMRoyalParasiteComponent>(itemUid.Value))
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-wrong-parasite-type-regular"), uid, args.Performer);
            return;
        }

        var totalRoyalParasites = component.CurRoyalParasites + component.CurRoyalParasitesInHands;
        if (totalRoyalParasites <= 0)
        {
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("rmc-xeno-throw-no-royal-parasites"), uid, args.Performer);
            return;
        }

        args.Handled = true;
    }
}
