using Content.Shared._CCM.Alert;
using Content.Shared.Alert;
using Robust.Shared.GameStates;

namespace Content.Server.Alert;

internal sealed class ServerAlertsSystem : AlertsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlertsComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(Entity<AlertsComponent> alerts, ref ComponentGetState args)
    {
        // CCM-change-start
        // Relay: build display alerts for owner, optionally including relay-source alerts.
        var display = new Dictionary<AlertKey, AlertState>(alerts.Comp.Alerts);

        if (TryComp<CCMAlertsDisplayRelayComponent>(alerts.Owner, out var relay) && relay.Source is { } src &&
            TryComp<AlertsComponent>(src, out var srcAlerts))
        {
            foreach (var (key, state) in srcAlerts.Alerts)
            {
                display[key] = state;
            }
        }
        //CCM-change-end

        // TODO: Use sourcegen when clone-state bug fixed.
        args.State = new AlertComponentState(display); //CCM-change
    }
}
