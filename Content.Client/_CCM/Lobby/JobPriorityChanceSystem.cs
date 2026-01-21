using Content.Client.Lobby;
using Content.Shared._CCM.Preferences;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Client._CCM.Lobby;

public sealed class JobPriorityChanceSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<JobPriorityChancesEvent>(OnJobPriorityChances);
    }

    private void OnJobPriorityChances(JobPriorityChancesEvent ev)
    {
        var controller = _ui.GetUIController<LobbyUIController>();
        controller.UpdateJobPriorityChances(ev.CharacterSlot, ev.Chances);
    }
}

// # CCM priority rework
