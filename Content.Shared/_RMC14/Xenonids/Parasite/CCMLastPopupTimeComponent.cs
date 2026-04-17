using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[RegisterComponent]
public sealed partial class CCMLastPopupTimeComponent : Component
{
    [DataField]
    public TimeSpan LastPopupTime;
}
