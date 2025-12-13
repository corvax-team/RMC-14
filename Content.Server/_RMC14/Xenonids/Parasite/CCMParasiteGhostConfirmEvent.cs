using Robust.Shared.Serialization;

namespace Content.Server._RMC14.Xenonids.Parasite;

[Serializable, NetSerializable]
public sealed record class CCMParasiteGhostConfirmEvent
{
    public int ConfirmId { get; set; }
}
