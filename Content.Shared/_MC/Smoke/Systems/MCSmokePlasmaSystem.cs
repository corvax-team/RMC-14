using Content.Shared._MC.Smoke.Components;
using Content.Shared._RMC14.Xenonids.Plasma;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokePlasmaSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokePlasmaComponent, MCSmokeEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<MCSmokePlasmaComponent> entity, ref MCSmokeEffectEvent args)
    {
        if (!TryComp<XenoPlasmaComponent>(args.TargetUid, out var plasmaComp))
            return;
        
        var xeno = new Entity<XenoPlasmaComponent>(args.TargetUid, plasmaComp);

        var current = plasmaComp.Plasma;
        if (current <= 0)
            return;

        var amount = entity.Comp.Amount + entity.Comp.Multiplier * current;

        _xenoPlasma.RemovePlasma(xeno, amount);
    }
}