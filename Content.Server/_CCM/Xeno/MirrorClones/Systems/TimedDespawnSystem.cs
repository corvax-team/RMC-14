using Content.Server._CCM.Xeno.MirrorClones.Components;

namespace Content.Server._CCM.Xeno.MirrorClones.Systems;

public sealed class TimedDespawnSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<CCMTimedDespawnComponent>();

        while (query.MoveNext(out var uid, out var td))
        {
            td.Accumulator += frameTime;

            if (td.Accumulator >= td.Lifetime)
                Del(uid);
        }
    }
}
