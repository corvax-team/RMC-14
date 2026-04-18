using Content.Shared.Camera;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._RMC14.Vehicle;

public sealed class RMCVehicleGunnerViewSystem : EntitySystem
{
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RMCVehicleGunnerViewUserComponent, GetEyePvsScaleEvent>(OnGetEyePvsScale);
        SubscribeLocalEvent<RMCVehicleGunnerViewUserComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<RMCVehicleGunnerViewUserComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RMCVehicleGunnerViewUserComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnGetEyePvsScale(Entity<RMCVehicleGunnerViewUserComponent> ent, ref GetEyePvsScaleEvent args)
    {
        args.Scale += ent.Comp.PvsScale + ent.Comp.CursorPvsIncrease;
    }

    private void OnHandleState(Entity<RMCVehicleGunnerViewUserComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _eye.UpdatePvsScale(ent.Owner);
    }

    private void OnStartup(Entity<RMCVehicleGunnerViewUserComponent> ent, ref ComponentStartup args)
    {
        if (HasComp<ContentEyeComponent>(ent)) // CCM14
            _eye.UpdatePvsScale(ent.Owner);
    }

    private void OnShutdown(Entity<RMCVehicleGunnerViewUserComponent> ent, ref ComponentShutdown args)
    {
        if (HasComp<ContentEyeComponent>(ent)) // CCM14
            _eye.UpdatePvsScale(ent.Owner);
    }
}
