using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;
using System;

namespace Content.Shared._CCM14.Xenonids.Screech;

public sealed class XenoScreechConfusionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly System.Random _random = new();

    // Configuration
    private const float FailChance = 0.3f;
    private const float DelayChance = 0.4f;
    private const float InteractionFailChance = 0.25f;
    private static readonly TimeSpan ActionDelay = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to projectile shooting events
        SubscribeLocalEvent<ProjectileShotEvent>(OnProjectileShot);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        
        var query = EntityQueryEnumerator<XenoScreechConfusionComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime > comp.ExpireAt)
            {
                RemComp<XenoScreechConfusionComponent>(uid);
            }
        }
    }

    /// <summary>
    /// Handles projectile shooting - adds random failures and delays
    /// </summary>
    private void OnProjectileShot(ref ProjectileShotEvent args)
    {
        if (args.Shooter is not EntityUid shooter)
            return;

        if (!TryComp(shooter, out XenoScreechConfusionComponent? comp) || comp is null)
            return;

        if (_timing.CurTime > comp.ExpireAt)
            return;

        // Random chance to fail completely
        if (_random.NextSingle() < FailChance)
        {
            args = new ProjectileShotEvent(shooter, false);
            _popup.PopupEntity("Вы дезориентированы и не можете стрелять!", shooter, shooter);
            return;
        }

        // Random chance to add delay
        if (_random.NextSingle() < DelayChance)
        {
            if (_timing.CurTime < comp.NextAllowedAction)
            {
                args = new ProjectileShotEvent(shooter, false);
                _popup.PopupEntity("Вы слишком дезориентированы для прицеливания!", shooter, shooter);
                return;
            }
            comp.NextAllowedAction = _timing.CurTime + ActionDelay;
        }
    }

    /// <summary>
    /// Called on entities with XenoScreechConfusionComponent to apply random interaction failures
    /// </summary>
    public void ProcessConfusion(EntityUid uid, XenoScreechConfusionComponent comp)
    {
        if (_timing.CurTime > comp.ExpireAt)
            return;

        // Random chance to show confusion message
        if (_random.NextSingle() < InteractionFailChance)
        {
            _popup.PopupEntity("Вы дезориентированы!", uid, uid);
        }
    }
}