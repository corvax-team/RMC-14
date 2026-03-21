using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRavageComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.Zero; // DoAfter

    [DataField, AutoNetworkedField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(6); 

    [ViewVariables] 
    public TimeSpan NextUse = TimeSpan.Zero; 

    [DataField, AutoNetworkedField]
    public EntProtoId EffectEntId = "MCEffectXenoSlash";

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> EffectEmote = "XenoRoar";
}