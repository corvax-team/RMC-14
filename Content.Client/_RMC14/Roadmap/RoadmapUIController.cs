using Content.Client.Credits;
using Content.Client.Lobby;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Info;
using Content.Shared.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;

namespace Content.Client._RMC14.Roadmap;

public sealed class RoadmapUIController : UIController, IOnStateEntered<LobbyState>
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly InfoUIController _infoUIController = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;

    private RoadmapWindow? _window;
    private bool _shown;

    public override void Initialize()
    {
        base.Initialize();
        _infoUIController.Accepted += OnAccepted;
    }

    public void OnStateEntered(LobbyState state)
    {
        // Do not auto-open roadmap on state entry.
    }

    private void OnAccepted()
    {
        // Do not auto-open roadmap after accepting rules.
    }

    public void ToggleRoadmap()
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
            return;
        }

        _shown = true;
        _window = new RoadmapWindow();
        _window.OnClose += () => _window = null;

        _window.CreditsButton.StyleClasses.Add(StyleBase.ButtonCaution);
        _window.CreditsButton.OnPressed += _ => new CreditsWindow().OpenCentered();

        _window.OpenCentered();
    }
}
