using Content.Shared._CCM.FpvDrone;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CCM.FpvDrone;

public sealed class FpvDroneOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;

    private FpvDroneOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new FpvDroneOverlay(_protoMan, _timing, _entMan, _playerMan);
        _overlays.AddOverlay(_overlay);
    }

    private sealed class FpvDroneOverlay(IPrototypeManager protoMan, IGameTiming timing, IEntityManager entMan, IPlayerManager playerMan) : Overlay
    {
        private readonly ShaderInstance _shader =
            protoMan.Index<ShaderPrototype>(FpvDroneConstants.ShaderId).InstanceUnique();

        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        public override bool RequestScreenTexture => true;

        protected override bool BeforeDraw(in OverlayDrawArgs args)
        {
            if (playerMan.LocalEntity is not { } player)
                return false;

            if (!entMan.TryGetComponent<FpvDroneLaptopWatcherComponent>(player, out var watcher))
                return false;

            if (watcher.CurrentDrone is not { } droneNet)
                return false;

            if (!entMan.TryGetEntity(droneNet, out var droneUid))
                return false;

            if (!entMan.TryGetComponent<EyeComponent>(droneUid.Value, out var eye))
                return false;

            if (args.Viewport.Eye != eye.Eye)
                return false;

            return true;
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null || args.Viewport.Eye == null)
                return;

            var handle = args.WorldHandle;

            _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _shader.SetParameter("time", (float)timing.CurTime.TotalSeconds);
            _shader.SetParameter("renderScale", args.Viewport.RenderScale * args.Viewport.Eye.Scale);
            _shader.SetParameter("active", true);

            handle.UseShader(_shader);
            handle.DrawRect(args.WorldBounds, Color.White);
            handle.UseShader(null);
        }
    }
}
