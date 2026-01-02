using Content.Shared._CCM.Mech.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Mech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MechSoundboardSystem))]
public sealed partial class CCMMechSoundboardComponent : Component
{
    /// <summary>
    /// List of sounds that can be played
    /// </summary>
    [DataField("sounds"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<SoundCollectionSpecifier> Sounds = new();
}
