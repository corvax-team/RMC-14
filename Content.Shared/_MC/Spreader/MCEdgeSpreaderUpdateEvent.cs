using Robust.Shared.Collections;

namespace Content.Shared._MC.Spreader;

[ByRefEvent]
public record struct MCEdgeSpreaderUpdateEvent
{
    public ValueList<Vector2i> FreeTiles;
    public int Updates;
}
