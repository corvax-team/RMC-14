using Content.Client._CCM.Mech.Overlays;
using Content.Shared._CCM.Mech.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Mech;

public sealed class VignetteOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private VignetteOverlay _overlay = null!;
    private float _transitionTimer = 0f;
    private const float TransitionDuration = 20.0f;
    private bool _isActive = false;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new VignetteOverlay();

        SubscribeLocalEvent<CCMMechPilotComponent, ComponentStartup>(OnMechPilotStartup);
        SubscribeLocalEvent<CCMMechPilotComponent, ComponentRemove>(OnMechPilotRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_isActive && _transitionTimer < TransitionDuration)
        {
            _transitionTimer += frameTime;
            var progress = _transitionTimer / TransitionDuration;
            _overlay.UpdateTransition(progress);
        }
    }

    private void OnMechPilotStartup(EntityUid uid, CCMMechPilotComponent component, ComponentStartup args)
    {
        if (uid == _playerManager.LocalPlayer?.ControlledEntity)
        {
            ActivateVignette();
        }
    }

    private void OnMechPilotRemove(EntityUid uid, CCMMechPilotComponent component, ComponentRemove args)
    {
        if (uid == _playerManager.LocalPlayer?.ControlledEntity)
        {
            DeactivateVignette();
        }
    }

    private void ActivateVignette()
    {
        if (_isActive) return;

        _overlayManager.AddOverlay(_overlay);
        _transitionTimer = 0f;
        _isActive = true;
        _overlay.UpdateTransition(0f);
    }

    private void DeactivateVignette()
    {
        if (!_isActive) return;

        _overlayManager.RemoveOverlay(_overlay);
        _isActive = false;
        _transitionTimer = 0f;
    }
}
