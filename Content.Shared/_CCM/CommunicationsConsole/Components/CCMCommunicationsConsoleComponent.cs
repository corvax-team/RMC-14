using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CCM.CommunicationsConsole.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMCommunicationsConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId CrasherMarkerForERT = "CCMERTCrashMarkerComponent";

    [DataField, AutoNetworkedField]
    public bool ERTCalled { get; set; } = false;

    [DataField, AutoNetworkedField]
    public List<ResPath> MapPaths = new()
    {
        new ResPath("/Maps/_CCM14/Shuttle/ert_spp_shuttle_spp.yml"),
        new ResPath("/Maps/_CCM14/Shuttle/ert_spp_shuttle_spp.yml"),
        new ResPath("/Maps/_CCM14/Shuttle/ert_spp_shuttle_spp.yml"),
        new ResPath("/Maps/_CCM14/Shuttle/ert_pmc_shuttle_freelancer.yml"),
        new ResPath("/Maps/_CCM14/Shuttle/ert_pmc_shuttle_weya.yml"),
        new ResPath("/Maps/_CCM14/Shuttle/ert_pmc_shuttle_weya.yml")
        //new ResPath("/Maps/_CCM14/Shuttle/ert_pmc_shuttle_enemies.yml")
    };

    [DataField, AutoNetworkedField]
    public TimeSpan FTLFlyTime = TimeSpan.FromSeconds(200);
}
// thanks to _gadmin1 (discord) for the provided code
