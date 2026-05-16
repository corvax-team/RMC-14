using Content.Shared._CCM.Stats;
using Robust.Shared.Network;

namespace Content.Server._CCM.Stats;

[RegisterComponent]
public sealed partial class CCMStatsProjectileSourceComponent : Component
{
    public NetUserId UserId;
    public CCMStatsSide Side;
}
