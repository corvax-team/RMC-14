using Content.Shared._RMC14.Xenonids.Plasma;
using Robust.Shared.Audio.Systems;

using Content.Shared._RMC14.Xenonids.LayEgg;

namespace Content.Server._RMC14.Xenonids.LayEgg;

public sealed class LayEggSystem : SharedLayEggSystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LayEggComponent, LayEggActionEvent>(OnXenoLayAction);
    }


    private void OnXenoLayAction(Entity<LayEggComponent> xeno, ref LayEggActionEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp<TransformComponent>(args.Performer, out var transform))
            return;
        if (!TryComp<LayEggComponent>(args.Performer, out var lay))
            return;
        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;
    
        Spawn(lay.EntitieID, transform.Coordinates);
        args.Handled = true;
    }
}