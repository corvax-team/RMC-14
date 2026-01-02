using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared._RMC14.Xenonids.Projectile.Parasite;

/// <summary>
/// Allows a xeno to throw parasites using the "Throw Parasite" Action
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoParasiteThrowerComponent : Component
{
    public EntProtoId ParasitePrototype = "CMXenoParasite";
    public EntProtoId RoyalParasitePrototype = "CCMXenoRoyalParasite";

    [DataField, AutoNetworkedField]
    public int ReservedParasites = 0;

    [DataField]
    public float ParasiteThrowDistance = 4.0f;

    [DataField]
    public float RoyalParasiteThrowDistance = 6.0f;

    [DataField]
    public float ParasitePickupRange = 1.5f;

    [DataField]
    public float RoyalParasitePickupRange = 1.5f;

    [DataField, AutoNetworkedField]
    public int MaxParasites = 16;

    [DataField, AutoNetworkedField]
    public int CurParasites = 0;

    [DataField, AutoNetworkedField]
    public int MaxRoyalParasites = 4;

    [DataField, AutoNetworkedField]
    public int ReservedRoyalParasites = 0;

    [DataField, AutoNetworkedField]
    public int CurRoyalParasites = 0;

    [DataField, AutoNetworkedField]
    public int CurParasitesInHands = 0;

    [DataField, AutoNetworkedField]
    public int CurRoyalParasitesInHands = 0;

    [DataField]
    public TimeSpan ThrownParasiteStunDuration = TimeSpan.FromSeconds(7.5);

    [DataField]
    public TimeSpan ThrownRoyalParasiteStunDuration = TimeSpan.FromSeconds(4.5);

    [DataField]
    public TimeSpan ThrownParasiteCooldown = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan ThrownRoyalParasiteCooldown = TimeSpan.FromSeconds(3);

    [DataField]
    public int NumPositions = 4;

    [DataField, AutoNetworkedField]
    public bool[] VisiblePositions = [false, false, false, false];

    [ViewVariables]
    public EntityUid? LastThrownParasite;
}

[Serializable, NetSerializable]
public enum ParasiteOverlayVisuals
{
    States
}

[Serializable, NetSerializable]
public enum ParasiteOverlayLayers : int
{
    RightArm = 0,
    Head = 1,
    LeftArm = 2,
    Back = 3
}
