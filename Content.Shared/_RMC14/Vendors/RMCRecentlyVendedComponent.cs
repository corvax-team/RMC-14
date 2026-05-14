using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Vendors;

[RegisterComponent, UnsavedComponent]
[Access(typeof(SharedCMAutomatedVendorSystem))]
public sealed partial class RMCRecentlyVendedComponent : Component
{
    [DataField]
    public HashSet<EntityUid> PreventCollide = new();
}
