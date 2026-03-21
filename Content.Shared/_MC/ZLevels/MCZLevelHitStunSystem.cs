using Content.Shared._CE.ZLevels.Core.EntitySystems;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelFallStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelFallStunComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelFallStunComponent> entity, ref CEZLevelHitEvent args)
    {
        _stun.TryStun(target, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1f), true);
    }
}