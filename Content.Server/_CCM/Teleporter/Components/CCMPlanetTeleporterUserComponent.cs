using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Teleporter;

[RegisterComponent]
public sealed partial class CCMPlanetTeleporterUserComponent : Component
{
    [DataField]
    public EntityCoordinates? Origin;

    [DataField]
    public bool Teleported;

    [DataField]
    public TimeSpan NextUseAt;
}

