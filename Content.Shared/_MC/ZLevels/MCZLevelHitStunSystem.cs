using Content.Shared._CE.ZLevels.Core.EntitySystems;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelHitStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelHitStunComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelHitStunComponent> entity, ref CEZLevelHitEvent args)
    {
        _stun.TryStun(target, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1f), true);
    }
}