using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CCM.CommunicationsConsole.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMCommunicationsConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId CrasherMarkerForERT = "CCMMarkerERTCrash";

    [DataField, AutoNetworkedField]
    public bool ERTCalled { get; set; }

    [DataField, AutoNetworkedField]
    public List<ResPath> MapPaths { get; set; } = new();

    [DataField, AutoNetworkedField]
    public TimeSpan FTLFlyTime = TimeSpan.FromSeconds(200);
}
// thanks to _gadmin1 (discord) for the provided code
