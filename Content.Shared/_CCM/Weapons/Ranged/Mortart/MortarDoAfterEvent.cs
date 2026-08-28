using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Weapons.Ranged.Mortar;

[Serializable, NetSerializable]
public sealed partial class MortarDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity GunNet;
    public NetCoordinates TargetCoords;

    public MortarDoAfterEvent(NetEntity gunNet, NetCoordinates targetCoords)
    {
        GunNet = gunNet;
        TargetCoords = targetCoords;
    }
}
