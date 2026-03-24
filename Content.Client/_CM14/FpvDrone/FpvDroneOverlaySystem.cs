using Content.Shared._CM14.FpvDrone;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CM14.FpvDrone;

public sealed class FpvDroneOverlaySystem : EntitySystem
{
    [Dependency] private readonly ILightManager _light = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private FpvDroneOverlay? _overlay;
    private bool _overlayEnabled;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FpvDroneObserverComponent, LocalPlayerAttachedEvent>((_, _, _) =>
            UpdateOverlay(true));
        SubscribeLocalEvent<FpvDroneObserverComponent, ComponentRemove>((_, _, _) =>
            UpdateOverlay(false));
        SubscribeLocalEvent<FpvDroneObserverComponent, LocalPlayerDetachedEvent>((_, _, _) =>
            UpdateOverlay(false));
    }

    private void UpdateOverlay(bool enable)
    {
        if (_overlayEnabled == enable)
            return;

        _overlayEnabled = enable;

        if (enable)
        {
            if (_overlay == null)
            {
                _overlay = new FpvDroneOverlay(_protoMan, _timing);
                _overlays.AddOverlay(_overlay);
            }

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