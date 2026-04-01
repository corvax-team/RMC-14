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
        _stun.TrySlowdown(entity, entity.Comp.SlowTime, true);
        _stun.TryStun(entity, entity.Comp.StunTime, true);
    }
}