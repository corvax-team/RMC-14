using Robust.Shared.Timing;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.UserInterface;
using Content.Shared.Ghost;
using Content.Shared._RMC14.Xenonids.Egg;

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

    private void OnRoyalParasiteGetActivationVerbs(Entity<CCMRoyalParasiteComponent> parasite, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!HasComp<GhostComponent>(args.User))
            return;

        if (_mobState.IsDead(parasite) || HasComp<ParasiteSpentComponent>(parasite))
            return;

        var user = args.User;
        var verb = new ActivationVerb
        {
            Text = Loc.GetString("rmc-xeno-egg-royal-ghost-verb"),
            Act = () =>
            {
                _ui.TryOpenUi(parasite.Owner, XenoParasiteGhostUI.Key, user);
            },
        };

        args.Verbs.Add(verb);
    }
}
