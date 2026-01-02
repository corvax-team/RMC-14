using Content.Shared._CCM.CommunicationsConsole.Components;
using Content.Shared._CCM.CommunicationsConsole.UI;
using Content.Shared._RMC14.Marines.Announce;

namespace Content.Shared._CCM.CommunicationsConsole;

public abstract class CCMSharedCommunicationsConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedMarineAnnounceSystem _marineAnnounce = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMCommunicationsConsoleComponent, CCMCommunicationsConsoleERTCallBuiMessage>(OnRunMessage);
    }

    protected virtual void OnRunMessage(Entity<CCMCommunicationsConsoleComponent> entity, ref CCMCommunicationsConsoleERTCallBuiMessage args)
    {

    }
}
// thanks to _gadmin1 (discord) for the provided code
