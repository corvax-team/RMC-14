using Content.Shared._MC.Smoke.Components;
using Content.Shared._RMC14.Xenonids.Plasma;
using Robust.Shared.Network;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokePlasmaSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = null!;
    [Dependency] private readonly INetManager _net = null!;

    public override void Initialize()
    {
        base.Initialize();

        if (_net.IsClient)
            return;

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
