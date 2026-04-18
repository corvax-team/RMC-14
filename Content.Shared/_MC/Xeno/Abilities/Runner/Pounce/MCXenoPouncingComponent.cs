using Robust.Shared.GameStates;
using System.Numerics;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPouncingComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Hit = new();

    [DataField, AutoNetworkedField]
    public TimeSpan End;

    [DataField, AutoNetworkedField]
    public bool ZigZag = false;

    [DataField, AutoNetworkedField]
    public float ZigZagAmplitude = 1.0f;

    [DataField, AutoNetworkedField]
    public float ZigZagFrequency = 4.0f;

    [DataField, AutoNetworkedField]
    public Vector2 Direction;

    [DataField, AutoNetworkedField]
    public TimeSpan StartTime;

    [DataField, AutoNetworkedField]
    public int Strength = 35;
}
