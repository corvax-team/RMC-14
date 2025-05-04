using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._TGMC14.FPV;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class FpvDroneExplosiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 1000 },
        }
    };
}
