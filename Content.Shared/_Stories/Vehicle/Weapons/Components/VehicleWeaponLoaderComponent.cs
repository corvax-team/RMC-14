using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleWeaponLoaderComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? SelectedHardpoint;

    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId<SkillDefinitionComponent>, int> Skills = new();

    [DataField, AutoNetworkedField]
    public SoundSpecifier? LoadSound = new SoundPathSpecifier("/Audio/_CCM/hydraulics_3.ogg");
}
