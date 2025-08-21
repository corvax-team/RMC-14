using Content.Shared._RMC14.Marines.Announce;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._TGMC14.BluespaceBarrier;

public sealed class BluespaceBarrierSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedMarineAnnounceSystem _marineAnnounce = default!;

    private readonly List<NetEntity> _barriers = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<BluespaceBarrierComponent, MapInitEvent>(OnBarrierMapInit);
        SubscribeLocalEvent<BluespaceBarrierDisappearEvent>(OnBarrierDisappear);
    }

    private void OnBarrierMapInit(Entity<BluespaceBarrierComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.DisappearTime.HasValue)
            return;

        ent.Comp.DisappearAt = _timing.CurTime + ent.Comp.DisappearTime.Value;
    }

    private void OnBarrierDisappear(BluespaceBarrierDisappearEvent args)
    {
        var barriers = args.Barriers;

        if (barriers.Count <= 0)
            return;

        foreach (var barrier in barriers)
        {
            if (_net.IsClient)
                break;

            QueueDel(GetEntity(barrier));
        }

        _marineAnnounce.AnnounceToMarines(
            Loc.GetString("tgmc-bluespace-barrier-disappear"),
            new SoundPathSpecifier("/Audio/_TGMC14/Announcements/bluespace_barrier_downed.ogg"));
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
        var query = EntityQueryEnumerator<BluespaceBarrierComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.DisappearAt.HasValue)
                continue;

            if (time < comp.DisappearAt)
                continue;

            _barriers.Add(GetNetEntity(uid));
        }

        if (_barriers.Count <= 0)
            return;

        var ev = new BluespaceBarrierDisappearEvent(_barriers);
        RaiseLocalEvent(ev);

        _barriers.Clear();
    }
}
