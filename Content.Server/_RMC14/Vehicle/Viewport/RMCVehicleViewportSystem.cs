using Content.Shared._RMC14.Vehicle;
using Content.Shared._RMC14.Vehicle.Viewport;
using Content.Shared.Interaction;
using Content.Shared.Movement.Events;
using Robust.Shared.Network;

namespace Content.Server._RMC14.Vehicle.Viewport;

public sealed class RMCVehicleViewportSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly RMCVehicleSystem _vehicles = default!;
    [Dependency] private readonly RMCVehicleViewToggleSystem _viewToggle = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RMCVehicleViewportComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<RMCVehicleViewportComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<RMCVehicleViewportComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<RMCVehicleViewportUserComponent, MoveInputEvent>(OnUserMove);
        SubscribeLocalEvent<RMCVehicleViewportUserComponent, ComponentRemove>(OnUserComponentRemove); // CCM14
    }
    // CCM14-start
    private void OnUserComponentRemove(Entity<RMCVehicleViewportUserComponent> ent, ref ComponentRemove args)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.LifeStage >= ComponentLifeStage.Stopping)
            return;

        CloseViewport(ent.Owner, ent.Comp);
    }
    // CCM14-end
    private void OnActivate(Entity<RMCVehicleViewportComponent> ent, ref ActivateInWorldEvent args)
    {
        if (_net.IsClient || args.Handled)
            return;

        if (!_vehicles.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle is null)
            return;

        if (ToggleViewport(args.User, vehicle.Value, ent.Owner))
            args.Handled = true;
    }

    private void OnUserMove(Entity<RMCVehicleViewportUserComponent> ent, ref MoveInputEvent args)
    {
        if (_net.IsClient || !args.HasDirectionalMovement)
            return;

        CloseViewport(ent.Owner, ent.Comp);
    }

    private void OnInteractUsing(Entity<RMCVehicleViewportComponent> ent, ref InteractUsingEvent args)
    {
        if (_net.IsClient || args.Handled)
            return;

        if (!_vehicles.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle is null)
            return;

        if (ToggleViewport(args.User, vehicle.Value, ent.Owner))
            args.Handled = true;
    }

    private void OnInteractHand(Entity<RMCVehicleViewportComponent> ent, ref InteractHandEvent args)
    {
        if (_net.IsClient || args.Handled)
            return;

        if (!_vehicles.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle is null)
            return;

        if (ToggleViewport(args.User, vehicle.Value, ent.Owner))
            args.Handled = true;
    }

    private bool ToggleViewport(EntityUid user, EntityUid vehicle, EntityUid source)
    {
        if (TryComp(user, out RMCVehicleViewportUserComponent? existing))
        {
            CloseViewport(user, existing);
            return true;
        }

        var userState = EnsureComp<RMCVehicleViewportUserComponent>(user);
        if (TryComp(user, out EyeComponent? newEye))
            userState.PreviousTarget = newEye.Target;
        userState.Source = source;

        _eye.SetTarget(user, vehicle);
        _viewToggle.EnableViewToggle(user, vehicle, source, userState.PreviousTarget, isOutside: true);
        return true;
    }

    private void CloseViewport(EntityUid user, RMCVehicleViewportUserComponent? state = null)
    {
        state ??= CompOrNull<RMCVehicleViewportUserComponent>(user);
        if (state == null)
            return;

        if (!Exists(user) || TerminatingOrDeleted(user))
            return;

        if (TryComp(user, out EyeComponent? eye) && eye.LifeStage < ComponentLifeStage.Stopping)
            _eye.SetTarget(user, state.PreviousTarget, eye);

        if (state.Source is { } source && Exists(source) && !TerminatingOrDeleted(source))
            _viewToggle.DisableViewToggle(user, source);

        RemCompDeferred<RMCVehicleViewportUserComponent>(user);
    }
}
