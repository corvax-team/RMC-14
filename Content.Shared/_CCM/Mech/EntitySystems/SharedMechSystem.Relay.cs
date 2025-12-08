using Content.Shared.Interaction.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared._CCM.Mech.Components;

namespace Content.Shared._CCM.Mech.EntitySystems;

public abstract partial class SharedMechSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<CCMMechComponent, GettingAttackedAttemptEvent>(RelayRefToPilot);
        SubscribeLocalEvent<CCMMechComponent, AttemptPacifiedAttackEvent>(RelayRefToPilot);
    }

    private void RelayToPilot<T>(Entity<CCMMechComponent> uid, T args) where T : class
    {
        if (!Vehicle.TryGetOperator(uid.Owner, out var operatorEnt))
            return;

        var ev = new MechPilotRelayedEvent<T>(args);

        RaiseLocalEvent(operatorEnt.Value, ref ev);
    }

    private void RelayRefToPilot<T>(Entity<CCMMechComponent> uid, ref T args) where T :struct
    {
        if (!Vehicle.TryGetOperator(uid.Owner, out var operatorEnt))
            return;

        var ev = new MechPilotRelayedEvent<T>(args);

        RaiseLocalEvent(operatorEnt.Value, ref ev);

        args = ev.Args;
    }
}

[ByRefEvent]
public record struct MechPilotRelayedEvent<TEvent>(TEvent Args)
{
    public TEvent Args = Args;
}
