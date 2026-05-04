using Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud.Events.Action;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Energy;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud;

public sealed class MCXenoMelterShroudSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly XenoEnergySystem _xenoEnergy = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoMelterShroudComponent, MCXenoMelterShroudActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoMelterShroudComponent> entity, ref MCXenoMelterShroudActionEvent args)
    {
        var xeno = entity.Owner;
        if (args.PlasmaCost != 0 && !_xenoPlasma.TryRemovePlasmaPopup(xeno, args.PlasmaCost))
            return;

        if (args.EnergyCost != 0 && !_xenoEnergy.TryRemoveEnergyPopup(xeno, args.EnergyCost))
            return;

        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
        return;

        args.Handled = true;

        var smokeUid = EntityManager.SpawnEntity(entity.Comp.ShroudId, _transform.GetMapCoordinates(entity));
        if (!smokeUid.Valid)
           return;

        _rmcXenoHive.SetSameHive(entity.Owner, smokeUid);
        _audio.PlayPvs(entity.Comp.EffectSound, Transform(smokeUid).Coordinates);
    }
}
