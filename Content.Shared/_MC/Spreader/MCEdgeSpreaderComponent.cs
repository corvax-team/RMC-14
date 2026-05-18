using Robust.Shared.GameStates;

namespace Content.Shared._MC.Spreader;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCEdgeSpreaderComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.1f);

    public TimeSpan NextUpdate;

    [DataField]
    public int Range = 5;
}
