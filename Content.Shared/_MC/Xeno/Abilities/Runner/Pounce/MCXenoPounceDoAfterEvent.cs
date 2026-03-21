using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

[Serializable, NetSerializable]
public sealed partial class MCXenoPounceDoAfterEvent : DoAfterEvent
{
    public readonly MapCoordinates TargetCoordinates;

    public MCXenoPounceDoAfterEvent(MapCoordinates targetCoordinates)
    {
        TargetCoordinates = targetCoordinates;
    }

    public override DoAfterEvent Clone()
    {
        return new MCXenoPounceDoAfterEvent(TargetCoordinates);
    }
}