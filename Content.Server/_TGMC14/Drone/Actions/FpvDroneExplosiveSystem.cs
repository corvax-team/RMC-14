using Content.Server.Explosion.EntitySystems;
using Content.Shared._TGMC14.FPV;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Server._TGMC14.FPV;

public sealed class FpvDroneExplosiveSystem : SharedFpvDroneExplosiveSystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FpvDroneExplosiveComponent, FpvDroneExplosiveEvent>(OnFPVExsplosiveAction);
    }

private void OnFPVExsplosiveAction(EntityUid uid, FpvDroneExplosiveComponent comp, FpvDroneExplosiveEvent args)
{
    if (args.Handled)
        return;

    _explosionSystem.QueueExplosion(uid, "Default", 200f, 5f, 100f,
        canCreateVacuum: false, user: uid, addLog: true);
}
}