using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Stunnable;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelFallStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelFallStunComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelFallStunComponent> entity, ref CEZLevelHitEvent args)
    {
        _stun.TryStun(entity, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(entity, TimeSpan.FromSeconds(1f), true);
    }
}