using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

[Serializable, NetSerializable]
public sealed partial class MCXenoRavageDoAfterEvent : DoAfterEvent
{
    public readonly MapCoordinates TargetCoordinates;

    public MCXenoRavageDoAfterEvent(MapCoordinates targetCoordinates)
    {
        TargetCoordinates = targetCoordinates;
    }

    public override DoAfterEvent Clone()
    {
        return new MCXenoRavageDoAfterEvent(TargetCoordinates);
    }
}