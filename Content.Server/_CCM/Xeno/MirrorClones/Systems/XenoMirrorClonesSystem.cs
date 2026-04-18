using System.Numerics;
using Content.Server._CCM.Xeno.MirrorClones.Components;
using Content.Shared._CCM.Actions.Events;
using Content.Shared._CCM.Xenonids.MirrorClones;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Xenonids.Plasma;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class XenoMirrorClonesSystem : EntitySystem
{
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    
    private const float ActiveSeconds = 10f;
    private const int ExtraDamage = 5;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MirrorClonesComponent, XenoMirrorClonesActionEvent>(OnMirrorClonesAction);
    }

    private void OnMirrorClonesAction(Entity<MirrorClonesComponent> xeno, ref XenoMirrorClonesActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, args.PlasmaCost))
            return;

        if (!_rmcActions.TryUseAction(args))
            return;

        var active = EnsureComp<MirrorClonesActiveComponent>(xeno.Owner);
        active.TimeLeft = ActiveSeconds;
        active.GeneticDamage = ExtraDamage;

        SpawnClones(xeno.Owner, "CCMXenoHunterMirrorClone");

        args.Handled = true;
    }

    private void SpawnClones(EntityUid original, string clonePrototype)
    {
        if (!TryComp(original, out TransformComponent? xform))
            return;

        var baseCoords = xform.Coordinates;

        const float side = 0.60f;
        const float back = -0.10f;

        var offsets = new[]
        {
            new Vector2(side, back),
            new Vector2(-side, back),
        };

        foreach (var off in offsets)
        {
            var clone = Spawn(clonePrototype, baseCoords.Offset(off));

            EnsureComp<MirrorCloneComponent>(clone).Original = original;

            var follow = EnsureComp<FollowEntityComponent>(clone);
            follow.Target = original;

            follow.RotateWithTarget = false;
            follow.Offset = off;

            follow.FollowStrength = 45f;
            follow.TeleportDistance = 1.0f;
        }
    }
}
