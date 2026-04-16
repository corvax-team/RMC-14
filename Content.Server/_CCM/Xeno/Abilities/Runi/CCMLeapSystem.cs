using System.Numerics;
using Content.Shared._CCM.Xeno.Abilities.ZigZagPounce;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Xeno.Abilities.ZigZagPounce;

public sealed class CCMZigZagPounceSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMZigZagPounceComponent, CCMZigZagPounceActionEvent>(OnAction);
    }

    private void OnAction(Entity<CCMZigZagPounceComponent> ent, ref CCMZigZagPounceActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);

        var dir = target.Position - origin.Position;

        var length = dir.Length();
        if (length < 0.001f)
            return;

        if (!TryComp(ent, out PhysicsComponent? physics))
            return;

        var forward = dir / length;
        var side = new Vector2(-forward.Y, forward.X);

        var distance = MathF.Min(length, ent.Comp.MaxDistance);

        var time = (float)_timing.CurTime.TotalSeconds;

        var zigPhase = MathF.Sin(time * ent.Comp.ZigZagFrequency);
        var zig = zigPhase >= 0f ? 1f : -1f;

        var impulse =
            forward * ent.Comp.Strength * physics.Mass +
            side * ent.Comp.ZigZagAmplitude * ent.Comp.Strength * physics.Mass * zig;

        _physics.ApplyLinearImpulse(ent, impulse, body: physics);
    }
}