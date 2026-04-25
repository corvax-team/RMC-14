// CM14 rework: non-RMC edit marker.
using Content.Shared._CCM.Sponsorship;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.IoC;

namespace Content.Client._CCM.Sponsorship;

[UsedImplicitly]
public sealed class CCMCustomizationUIController : UIController
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private CCMCustomizationWindow? _window;
    private CCMSponsorshipSystem? _sponsorshipSystem;
    private CCMCustomizationSystem? _customizationSystem;
    private bool _subscribed;

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

    public void OpenWindow()
    {
        EnsureSystems();
        EnsureWindow();
        _window?.OpenCenteredAnimated();
        _sponsorshipSystem?.RequestStatus();
        _customizationSystem?.RequestCustomization();
    }

    private void EnsureSystems()
    {
        if (_subscribed)
            return;

        _sponsorshipSystem = _entManager.System<CCMSponsorshipSystem>();
        _customizationSystem = _entManager.System<CCMCustomizationSystem>();
        _sponsorshipSystem.StatusReceived += OnStatusReceived;
        _customizationSystem.CustomizationReceived += OnCustomizationReceived;
        _subscribed = true;
    }

    private void EnsureWindow()
    {
        if (_window != null && !_window.Disposed)
            return;

        _window = UIManager.CreateWindow<CCMCustomizationWindow>();
        _window.SaveRequested += OnSaveRequested;

        if (_sponsorshipSystem?.LatestStatus is { } status)
            _window.SetStatus(status);

        if (_customizationSystem?.LatestSnapshot is { } snapshot)
            _window.SetSnapshot(snapshot);
    }

    private void OnStatusReceived(CCMSponsorshipStatusSnapshot snapshot)
    {
        _window?.SetStatus(snapshot);
    }

    private void OnCustomizationReceived(CCMCustomizationSnapshot snapshot)
    {
        _window?.SetSnapshot(snapshot);
    }

    private void OnSaveRequested(CCMCustomizationSnapshot snapshot)
    {
        _customizationSystem?.SaveCustomization(snapshot);
    }
}
