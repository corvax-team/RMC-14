using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Content.Shared._CCM.Silencer;
using Content.Shared._RMC14.Marines;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Silencer;

public sealed class CCMEchoJammerSystem : SharedCCMEchoJammerSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    private readonly HashSet<Entity<MarineComponent>> _marines = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMEchoJammerComponent, CCMEchoJammerToggleActionEvent>(OnToggleAction);
        SubscribeLocalEvent<CCMEchoJammerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnToggleAction(Entity<CCMEchoJammerComponent> ent, ref CCMEchoJammerToggleActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var user = args.Performer;

        if (!ent.Comp.Enabled &&
            TryComp(ent, out CCMSilencerUseWhitelistComponent? restriction) &&
            !_whitelist.IsValid(restriction.Whitelist, user))
        {
            _popup.PopupEntity(Loc.GetString(restriction.DenyPopup, ("item", ent.Owner)), user, user, PopupType.SmallCaution);
            return;
        }

        if (ent.Comp.Enabled)
        {
            SetEnabled(ent, false);
            _audio.PlayPvs(ent.Comp.ToggleOffSound, ent);
            _popup.PopupEntity(Loc.GetString("ccm-silencer-echo-off"), user, user);
            return;
        }

        if (!TryGetWearer(ent, out _))
        {
            _popup.PopupEntity(Loc.GetString("ccm-silencer-echo-not-worn"), user, user, PopupType.SmallCaution);
            return;
        }

        if (!_powerCell.TryGetBatteryFromSlot(ent, out var battery) || battery.CurrentCharge <= 0)
        {
            _audio.PlayPvs(ent.Comp.EmptySound, ent);
            _popup.PopupEntity(Loc.GetString("ccm-silencer-echo-no-charge"), user, user, PopupType.SmallCaution);
            return;
        }

        SetEnabled(ent, true);
        ent.Comp.NextUpdate = _timing.CurTime;
        _audio.PlayPvs(ent.Comp.ToggleOnSound, ent);
        _popup.PopupEntity(Loc.GetString("ccm-silencer-echo-on"), user, user);
    }

    private void OnExamined(Entity<CCMEchoJammerComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(CCMEchoJammerComponent)))
        {
            args.PushMarkup(Loc.GetString(ent.Comp.Enabled ? "ccm-silencer-echo-examine-on" : "ccm-silencer-echo-examine-off"));

            if (_powerCell.TryGetBatteryFromSlot(ent, out var battery) && battery.MaxCharge > 0)
            {
                var fraction = battery.CurrentCharge / battery.MaxCharge;
                var time = TimeSpan.FromSeconds(Math.Ceiling(fraction * ent.Comp.RunTime.TotalSeconds));
                args.PushMarkup(Loc.GetString("ccm-silencer-echo-examine-charge",
                    ("percent", (int) Math.Round(fraction * 100)),
                    ("time", $"{(int) time.TotalMinutes}:{time.Seconds:D2}")));
            }
            else
            {
                args.PushMarkup(Loc.GetString("ccm-silencer-echo-examine-no-cell"));
            }
        }
    }

    private bool TryGetWearer(EntityUid jammer, out EntityUid wearer)
    {
        wearer = default;
        if (!_inventory.TryGetContainingSlot(jammer, out var slot) ||
            (slot.SlotFlags & SlotFlags.BACK) == 0)
        {
            return false;
        }

        wearer = Transform(jammer).ParentUid;
        return wearer.IsValid();
    }

    private void Disable(Entity<CCMEchoJammerComponent> ent, string? message)
    {
        SetEnabled(ent, false);
        _audio.PlayPvs(ent.Comp.EmptySound, ent);

        if (message != null && TryGetWearer(ent, out var wearer))
            _popup.PopupEntity(Loc.GetString(message), wearer, wearer, PopupType.SmallCaution);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var jammers = EntityQueryEnumerator<CCMEchoJammerComponent>();
        while (jammers.MoveNext(out var uid, out var jammer))
        {
            if (!jammer.Enabled)
                continue;

            var ent = (uid, jammer);

            if (!_powerCell.TryGetBatteryFromSlot(uid, out var batteryEnt, out var battery))
            {
                Disable(ent, "ccm-silencer-echo-no-charge");
                continue;
            }

            var runSeconds = (float) jammer.RunTime.TotalSeconds;
            if (runSeconds > 0)
            {
                var drain = battery.MaxCharge / runSeconds;

                // Self-recharging cells must not extend the run time.
                if (TryComp(batteryEnt.Value, out BatterySelfRechargerComponent? recharger) && recharger.AutoRecharge)
                    drain += recharger.AutoRechargeRate;

                _battery.UseCharge(batteryEnt.Value, drain * frameTime, battery);
            }

            if (battery.CurrentCharge <= 0)
            {
                Disable(ent, "ccm-silencer-echo-depleted");
                continue;
            }

            if (!TryGetWearer(uid, out var wearer))
            {
                Disable(ent, null);
                continue;
            }

            if (now < jammer.NextUpdate)
                continue;

            jammer.NextUpdate = now + jammer.UpdateInterval;
            JamAround(wearer, jammer, now);
        }

        var jammed = EntityQueryEnumerator<CCMEchoJammedComponent>();
        while (jammed.MoveNext(out var uid, out var comp))
        {
            if (comp.ExpiresAt <= now)
                RemCompDeferred<CCMEchoJammedComponent>(uid);
        }
    }

    private void JamAround(EntityUid wearer, CCMEchoJammerComponent jammer, TimeSpan now)
    {
        var expires = now + jammer.JamLinger;

        Jam(wearer, expires);

        var center = _transform.GetMapCoordinates(wearer);
        var half = jammer.HalfExtent;

        _marines.Clear();
        _lookup.GetEntitiesInRange(center, half * 1.42f, _marines);

        foreach (var marine in _marines)
        {
            if (_mobState.IsDead(marine))
                continue;

            var pos = _transform.GetMapCoordinates(marine).Position;
            if (Math.Abs(pos.X - center.Position.X) > half ||
                Math.Abs(pos.Y - center.Position.Y) > half)
            {
                continue;
            }

            Jam(marine, expires);
        }
    }

    private void Jam(EntityUid target, TimeSpan expires)
    {
        var jammed = EnsureComp<CCMEchoJammedComponent>(target);
        if (jammed.ExpiresAt < expires)
            jammed.ExpiresAt = expires;
    }
}
