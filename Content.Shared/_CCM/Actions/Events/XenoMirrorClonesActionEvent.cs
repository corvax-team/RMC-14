using Content.Shared.Actions;
using Content.Shared.FixedPoint;

namespace Content.Shared._CCM.Actions.Events;

public sealed partial class XenoMirrorClonesActionEvent : InstantActionEvent
{
    [DataField] public FixedPoint2 PlasmaCost = 50;
}
