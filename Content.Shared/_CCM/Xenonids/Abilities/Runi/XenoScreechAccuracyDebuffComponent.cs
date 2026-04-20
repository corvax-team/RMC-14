using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM14.Xenonids.Screech;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoScreechAccuracyDebuffComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<(FixedPoint2 Multiplier, TimeSpan ExpiresAt)> Received = new();

    [DataField]
    public FixedPoint2 AccuracyModifier = -1.0;

    [DataField]
    public FixedPoint2 AccuracyPerTileModifier = -1.0;
}
