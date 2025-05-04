using Content.Shared.FpvDrone;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Client.FpvDrone
{
    public sealed class FpvDroneOverlaySystem : EntitySystem
    {
        [Dependency] private readonly IOverlayManager _overlays = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;

        private FpvDroneOverlay? _overlay;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<FpvDroneSetOverlayEvent>(OnSetOverlay);
        }

        private void OnSetOverlay(FpvDroneSetOverlayEvent ev)
        {
            if (ev.Enable)
            {
                if (_overlay == null)
                {
                    _overlay = new FpvDroneOverlay(_protoMan);
                    _overlays.AddOverlay(_overlay);
                }
            }
            else
            {
                if (_overlay != null)
                {
                    _overlays.RemoveOverlay(_overlay);
                    _overlay = null;
                }
            }
        }
    }

    public sealed class FpvDroneOverlay : Overlay
    {
        public override OverlaySpace Space => OverlaySpace.ScreenSpace;
        private readonly ShaderInstance _shader;

        public FpvDroneOverlay(IPrototypeManager protoMan)
        {
            var shaderPrototype = protoMan.Index<ShaderPrototype>("FlashedEffect");
            _shader = shaderPrototype?.Instance() ?? throw new InvalidOperationException("Shader not found");
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            var handle = args.ScreenHandle;
            var bounds = args.ViewportBounds;

            handle.UseShader(_shader);
            handle.DrawRect(bounds, Color.White); // Прямоугольник на экране
            handle.UseShader(null); // Отключаем шейдер
        }
    }
}