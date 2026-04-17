using Robust.Shared.Timing;
using Content.Shared.Popups;

namespace Content.Shared._RMC14.Xenonids.Parasite;

public abstract partial class SharedXenoParasiteSystem
{
    private bool CanRoyalInfect(EntityUid parasiteUid, EntityUid targetUid)
    {
        if (!TryComp<CCMRoyalParasiteComponent>(parasiteUid, out var royalComp))
            return true;

        if (HasComp<ParasiteSpentComponent>(parasiteUid))
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-failed-cant-infect", ("target", targetUid)), parasiteUid);
            return false;
        }

        if (royalComp.InfectionCount >= royalComp.MaxInfections)
        {
            _popup.PopupEntity(Loc.GetString("rmc-xeno-royal-parasite-no-infections-left"), parasiteUid);
            return false;
        }

        if (TryComp<ParasiteTiredOutComponent>(parasiteUid, out var tired))
        {
            var currentTime = _timing.CurTime;
            var timeLeft = tired.CooldownEndTime - currentTime;

            if (timeLeft > TimeSpan.Zero)
            {
                _popup.PopupEntity(Loc.GetString("rmc-xeno-royal-parasite-cooldown", ("seconds", Math.Ceiling(timeLeft.TotalSeconds).ToString())), parasiteUid);
                return false;
            }

            RemComp<ParasiteTiredOutComponent>(parasiteUid);
        }

        return true;
    }

    private void HandleRoyalInfectionSuccess(EntityUid parasiteUid)
    {
        if (!TryComp<CCMRoyalParasiteComponent>(parasiteUid, out var royalComp))
            return;

        royalComp.InfectionCount++;

        var remainingInfections = royalComp.MaxInfections - royalComp.InfectionCount;
        if (remainingInfections > 0)
        {
            var comp = EnsureComp<ParasiteTiredOutComponent>(parasiteUid);
            comp.CooldownEndTime = _timing.CurTime + royalComp.InfectionCooldown;

            _popup.PopupEntity(Loc.GetString("rmc-xeno-royal-parasite-infections-remaining", ("count", remainingInfections)), parasiteUid);
        }
        else
        {
            EnsureComp<ParasiteSpentComponent>(parasiteUid);
            _popup.PopupEntity(Loc.GetString("rmc-xeno-parasite-royal-final-death"), parasiteUid);

            RemComp<ParasiteTiredOutComponent>(parasiteUid);
        }

        EntityManager.Dirty(parasiteUid, royalComp);
    }
}
