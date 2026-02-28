using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Pathogen.Protomorphs.Components;

[RegisterComponent]
public sealed partial class BloodbursterComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionBirthBloodburster";

    [DataField]
    public EntityUid? ActionId;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Slash", 1000 }
        }
    };
}
