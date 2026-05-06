using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xeno.MirrorClones.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FollowEntityComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Target;

    [DataField, AutoNetworkedField] 
    public float FollowStrength = 10f;
    
    [DataField, AutoNetworkedField] 
    public float TeleportDistance = 2.5f;

    [DataField, AutoNetworkedField] 
    public Vector2 Offset = Vector2.Zero;

    [DataField, AutoNetworkedField] 
    public Vector2 LocalOffset = Vector2.Zero;

    [DataField, AutoNetworkedField] 
    public bool RotateWithTarget = true;

    [DataField, AutoNetworkedField] 
    public Angle LockedAngle;
}
