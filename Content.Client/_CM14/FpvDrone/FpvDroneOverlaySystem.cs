using Content.Shared._CM14.FpvDrone;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CM14.FpvDrone;

public sealed class FpvDroneOverlaySystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ILightManager _light = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private FpvDroneOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FpvDroneSetOverlayEvent>(ev => UpdateOverlay(ev.Enable));
    }

    private void UpdateOverlay(bool enable)
    {
        if (enable)
        {
            if (_overlay != null)
                return;

            _overlay = new FpvDroneOverlay(_protoMan, _timing);
            _overlays.AddOverlay(_overlay);

            _light.DrawLighting = false;
        }
        else
        {
            if (_overlay != null)
            {
                _overlays.RemoveOverlay(_overlay);
                _overlay = null;
            }

            _light.DrawLighting = true;
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var player = _player.LocalEntity;

        if (player == null)
        {
            UpdateOverlay(false);
            return;
        }

        if (!_entManager.HasComponent<FpvDroneScreenOverlayComponent>(player))
        {
            if (_overlay != null)
                UpdateOverlay(false);

            return;
        }

        if (_overlay == null)
            UpdateOverlay(true);
    }

    private sealed class FpvDroneOverlay(IPrototypeManager protoMan, IGameTiming timing) : Overlay
    {
        private readonly ShaderInstance _shader =
            protoMan.Index<ShaderPrototype>(FpvDroneConstants.ShaderId).InstanceUnique();

        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        public override bool RequestScreenTexture => true;

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null)
                return;

            var handle = args.WorldHandle;

            _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _shader.SetParameter("time", (float)timing.CurTime.TotalSeconds);

            handle.UseShader(_shader);
            handle.DrawRect(args.WorldBounds, Color.White);
            handle.UseShader(null);
        }
    }
}