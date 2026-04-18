using System.Linq;
using Content.Client.Message;
using Content.Shared._CCM.FpvDrone;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._CCM.FpvDrone;

[UsedImplicitly]
public sealed class FpvDroneLaptopBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    private FpvDroneLaptopWindow? _window;
    private NetEntity? _selectedDrone;
    private bool _controlEnabled;

    public FpvDroneLaptopBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<FpvDroneLaptopWindow>();
        _window.Title = Loc.GetString("cm-fpv-drone-laptop-title");
        _window.ControlButton.OnPressed += _ => SendMessage(new FpvDroneLaptopToggleControlBuiMsg());
        _window.DetonateButton.OnPressed += _ =>
        {
            if (_selectedDrone != null)
                SendMessage(new FpvDroneLaptopDetonateBuiMsg(_selectedDrone.Value));
        };
        _window.UnlinkButton.OnPressed += _ =>
        {
            if (_selectedDrone != null)
                SendMessage(new FpvDroneLaptopUnlinkBuiMsg(_selectedDrone.Value));
        };

        if (State is FpvDroneLaptopBuiState laptopState)
            UpdateState(laptopState);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is FpvDroneLaptopBuiState laptopState)
            UpdateState(laptopState);
    }

    private void UpdateState(FpvDroneLaptopBuiState state)
    {
        if (_window == null)
            return;

        if (_selectedDrone != null && state.Drones.All(d => d.Id != _selectedDrone))
        {
            _selectedDrone = null;
            _controlEnabled = false;
        }

        if (_selectedDrone == null && state.Drones.Count > 0)
            SelectDrone(state.Drones[0].Id, true);

        RebuildDroneList(state);
        UpdateSelectedPanel(state);
    }

    private void RebuildDroneList(FpvDroneLaptopBuiState state)
    {
        if (_window == null)
            return;

        _window.DroneListContainer.DisposeAllChildren();
        
        if (state.Drones.Count == 0)
        {
            _window.DroneListContainer.AddChild(new Label 
            { 
                Text = Loc.GetString("cm-fpv-drone-laptop-no-linked"),
                FontColorOverride = Color.FromHex("#8EA9C4"),
                Margin = new Thickness(0, 4)
            });
            return;
        }

        foreach (var drone in state.Drones)
        {
            var button = new Button
            {
                HorizontalExpand = true,
                Margin = new Thickness(0, 0, 0, 4),
                Text = $"{drone.Name}" 
            };

            button.ModulateSelfOverride = drone.Connected
                ? (_selectedDrone == drone.Id ? Color.FromHex("#315980") : Color.FromHex("#1B2A36"))
                : Color.FromHex("#3A1F1F");

            button.OnPressed += _ => SelectDrone(drone.Id);
            _window.DroneListContainer.AddChild(button);
        }
    }

    private void UpdateSelectedPanel(FpvDroneLaptopBuiState state)
    {
        if (_window == null)
            return;

        var selected = _selectedDrone != null
            ? state.Drones.FirstOrDefault(d => d.Id == _selectedDrone.Value)
            : null;

        if (selected == null)
        {
            _window.SelectedDroneLabel.SetMarkupPermissive($"[color=#5AA8FF][bold]{Loc.GetString("cm-fpv-drone-laptop-no-selection")}[/bold][/color]");
            _window.RoleValueLabel.Text = "-";
            _window.ConnectionValueLabel.SetMarkupPermissive($"[color=#8EA9C4]{Loc.GetString("cm-fpv-drone-laptop-select-drone") }[/color]");
            _window.PilotValueLabel.Text = "-";
            _window.HealthBar.Value = 0;
            _window.HealthLabel.Text = "-";
            _window.ControlButton.Disabled = true;
            _window.DetonateButton.Disabled = true;
            _window.UnlinkButton.Disabled = true;
            _window.CameraHintLabel.SetMarkupPermissive($"[color=#8EA9C4]{Loc.GetString("cm-fpv-drone-laptop-feed-idle")}[/color]");
            _window.CameraViewport.Eye = new FixedEye();
            return;
        }

        if (!selected.IsControlled)
            _controlEnabled = false;

        _window.SelectedDroneLabel.SetMarkupPermissive($"[color=#5AA8FF][bold]{selected.Name}[/bold][/color]");
        _window.RoleValueLabel.Text = selected.Role;
        _window.ConnectionValueLabel.SetMarkupPermissive(GetConnectionMarkup(selected));
        _window.PilotValueLabel.Text = selected.OperatorName ?? Loc.GetString("cm-fpv-drone-laptop-no-operator");
        _window.HealthBar.MinValue = 0;
        _window.HealthBar.MaxValue = Math.Max(1f, selected.MaxHealth);
        _window.HealthBar.Value = selected.Health;
        _window.HealthLabel.Text = $"{MathF.Round(selected.Health)}/{MathF.Round(selected.MaxHealth)}";
        _window.ControlButton.Disabled = !selected.Connected && !_controlEnabled;
        _window.ControlButton.Text = _controlEnabled
            ? Loc.GetString("cm-fpv-drone-laptop-release-control")
            : Loc.GetString("cm-fpv-drone-laptop-take-control");
        _window.DetonateButton.Disabled = !selected.CanDetonate || !selected.Connected;
        _window.UnlinkButton.Disabled = false;
        _window.CameraHintLabel.SetMarkupPermissive(_controlEnabled
            ? $"[color=#B7D5F7]{Loc.GetString("cm-fpv-drone-laptop-control-active") }[/color]"
            : $"[color=#8EA9C4]{Loc.GetString("cm-fpv-drone-laptop-feed-live") }[/color]");

        RefreshCamera();
    }

    private string GetConnectionMarkup(FpvDroneLaptopInfo selected)
    {
        if (selected.SignalLost || !selected.Connected)
            return $"[color=#E36B6B]{Loc.GetString("cm-fpv-drone-laptop-signal-lost") }[/color]";

        return selected.IsControlled
            ? $"[color=#7ED67E]{Loc.GetString("cm-fpv-drone-laptop-status-controlled") }[/color]"
            : $"[color=#5AA8FF]{Loc.GetString("cm-fpv-drone-laptop-status-standby") }[/color]";
    }

    private void SelectDrone(NetEntity drone, bool initial = false)
    {
        _selectedDrone = drone;
        _controlEnabled = false;
        SendMessage(new FpvDroneLaptopSelectDroneBuiMsg(drone));

        if (!initial && State is FpvDroneLaptopBuiState laptopState)
            UpdateSelectedPanel(laptopState);
    }

    private void RefreshCamera()
    {
        if (_window == null || _selectedDrone == null)
            return;

        if (!_entities.TryGetEntity(_selectedDrone.Value, out var droneUid) ||
            !_entities.TryGetComponent<EyeComponent>(droneUid.Value, out var eye))
        {
            _window.CameraViewport.Eye = new FixedEye();
            return;
        }

        var droneEye = eye.Eye;
        droneEye.DrawLight = false;
        droneEye.DrawFov = false;
        _window.CameraViewport.Eye = droneEye;
    }
}
