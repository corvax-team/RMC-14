using Content.Server.Explosion.EntitySystems;
using Content.Shared._TGMC14.FPV;
using Content.Shared.Damage;

namespace Content.Server._TGMC14.FPV;

public sealed class FpvDroneExplosiveSystem : SharedFpvDroneExplosiveSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneExplosiveComponent, FpvDroneExplosiveEvent>(OnFPVExsplosiveAction);
    }

    private void OnFPVExsplosiveAction(Entity<FpvDroneExplosiveComponent> ent, ref FpvDroneExplosiveEvent args)
    {
        if (args.Handled)
            return;

        _explosionSystem.QueueExplosion(ent, "Default", 200f, 5f, 100f, canCreateVacuum: false, user: ent,
            addLog: true);

        // _damageableSystem.TryChangeDamage(ent, ent.Comp.Damage);
    }
}
