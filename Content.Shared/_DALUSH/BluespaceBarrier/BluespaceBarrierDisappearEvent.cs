using Robust.Shared.Serialization;

namespace Content.Shared._TGMC14.BluespaceBarrier;

[Serializable, NetSerializable]
public sealed class BluespaceBarrierDisappearEvent : EntityEventArgs
{
    public List<NetEntity> Barriers;

    public BluespaceBarrierDisappearEvent(List<NetEntity> barriers)
    {
        Barriers = barriers;
    }
}
