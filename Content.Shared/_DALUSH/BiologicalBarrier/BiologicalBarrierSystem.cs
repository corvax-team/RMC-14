using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._TGMC14.BiologicalBarrier;

public sealed class BiologicalBarrierSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BiologicalBarrierComponent, MapInitEvent>(OnBarrierMapInit);
        SubscribeLocalEvent<BiologicalBarrierComponent, BiologicalBarrierDisappearEvent>(OnBarrierDisappear);
    }

    private void OnBarrierMapInit(Entity<BiologicalBarrierComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.DisappearWhen.HasValue)
            return;

        if (_player.PlayerCount < ent.Comp.DisappearWhen.Value)
            return;

        var ev = new BiologicalBarrierDisappearEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnBarrierDisappear(Entity<BiologicalBarrierComponent> ent, ref BiologicalBarrierDisappearEvent args)
    {
        if (_net.IsServer)
            QueueDel(ent);
    }
}
