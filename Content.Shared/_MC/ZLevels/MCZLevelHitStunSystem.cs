using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Stunnable;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelHitStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelHitStunComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelHitStunComponent> entity, ref CEZLevelHitEvent args)
    {
        _stun.TryStun(entity, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(entity, TimeSpan.FromSeconds(1f), true);
    }
}