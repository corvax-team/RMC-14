using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.CommunicationsConsole.ERT;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMERTCrashMarkerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(0.5f, 0.5f);
}
// thanks to _gadmin1 (discord) for the provided code
