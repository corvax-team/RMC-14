// CM14 rework: non-RMC edit marker.
using Content.Shared._CCM.Sponsorship;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.IoC;

namespace Content.Client._CCM.Sponsorship;

[UsedImplicitly]
public sealed class CCMSponsorshipUIController : UIController
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IUriOpener _uri = default!;

    private CCMSponsorshipWindow? _window;
    private CCMSponsorshipSystem? _system;
    private bool _subscribed;

    public void OpenWindow()
    {
        EnsureSystem();
        EnsureWindow();
        _window?.OpenCenteredAnimated();
        _system?.RequestStatus();
    }

    public void ToggleWindow()
    {
        EnsureWindow();
        if (_window == null)
            return;

        if (_window.IsOpen)
            _window.CloseAnimated();
        else
            OpenWindow();
    }

    private void EnsureSystem()
    {
        if (_subscribed)
            return;

        _system = _entManager.System<CCMSponsorshipSystem>();
        _system.StatusReceived += OnStatusReceived;
        _subscribed = true;
    }

    private void EnsureWindow()
    {
        if (_window != null && !_window.Disposed)
            return;

        _window = UIManager.CreateWindow<CCMSponsorshipWindow>();
        _window.OpenDonateRequested += OnOpenDonateRequested;
    }

    private void OnStatusReceived(CCMSponsorshipStatusSnapshot snapshot)
    {
        _window?.SetStatus(snapshot);
    }

    private void OnOpenDonateRequested(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            _uri.OpenUri(url);
    }
}
