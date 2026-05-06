using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MirrorCloneSwingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Duration = 0.14f;
    
    [DataField, AutoNetworkedField]
    public float Time = 0f;

    [DataField, AutoNetworkedField]
    public float LungeDistance = 0.16f;
}
