using Content.Shared._CCM.CommunicationsConsole.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.CommunicationsConsole.UI;

[UsedImplicitly]
public sealed class CCMCommunicationsConsoleBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private CCMCommunicationsConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CCMCommunicationsConsoleWindow>();
        _window.ERTCallButton.OnPressed += _ => SendMessage(new CCMCommunicationsConsoleERTCallBuiMessage());
    }
}
// thanks to _gadmin1 (discord) for the provided code
