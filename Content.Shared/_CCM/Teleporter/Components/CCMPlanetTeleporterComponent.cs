using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Teleporter;

[RegisterComponent]
public sealed partial class CCMPlanetTeleporterComponent : Component
{
    [DataField]
    public string PlanetMapId = "planet";

    [DataField]
    public int CooldownSeconds = 300;
}

