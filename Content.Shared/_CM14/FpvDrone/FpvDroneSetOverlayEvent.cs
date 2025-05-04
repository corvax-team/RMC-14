using Robust.Shared.Serialization;

namespace Content.Shared.FpvDrone
{
    [Serializable, NetSerializable]
    public sealed class FpvDroneSetOverlayEvent : EntityEventArgs
    {
        public bool Enable;

        public FpvDroneSetOverlayEvent(bool enable)
        {
            Enable = enable;
        }
    }
}