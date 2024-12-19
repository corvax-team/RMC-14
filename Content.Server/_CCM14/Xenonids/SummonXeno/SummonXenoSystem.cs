using Content.Shared._RMC14.Xenonids.Plasma;
using Robust.Shared.Audio.Systems;

using Content.Shared._RMC14.Xenonids.SummonXeno;

namespace Content.Server._RMC14.Xenonids.SummonXeno;

public sealed class SummonXenoSystem : SharedSummonXenoSystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SummonXenoComponent, SummonXenoActionEvent>(OnXenoSummonAction);
    }

    private void OnXenoSummonAction(Entity<SummonXenoComponent> xeno, ref SummonXenoActionEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp<TransformComponent>(args.Performer, out var transform))
            return;
        if (!TryComp<SummonXenoComponent>(args.Performer, out var summon))
            return;
        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        for (var i = 0; i < summon.Number; i++)
        {
            Spawn(summon.EntitieID, transform.Coordinates);
        }
        args.Handled = true;
    }
}