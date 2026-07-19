using Content.Shared._CCM.Xenonids.Screech;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Enums;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Content.Client._CCM.Xenonids.Screech;

public sealed class ScreechDizzyOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private ScreechDizzyOverlay? _overlay;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var local = _player.LocalEntity;

        if (local == null)
        {
            Disable();
            return;
        }


        if (!_ent.TryGetComponent<ScreechDizzyComponent>(local.Value, out var comp))
        {
            Disable();
            return;
        }

        var remaining = (float)(comp.EndTime - _timing.CurTime).TotalSeconds;

        if (remaining <= 0f)
        {
            Disable();
            return;
        }

        Enable();

        if (_overlay != null)
            _overlay.Intensity = Math.Clamp(remaining / 6f, 0f, 1f);
    }

    private void Enable()
    {
        if (_overlay != null)
            return;

        _overlay = new ScreechDizzyOverlay(_proto, _timing);
        _overlays.AddOverlay(_overlay);
    }

    private void Disable()
    {
        if (_overlay == null)
            return;

        _overlays.RemoveOverlay(_overlay);
        _overlay = null;
    }

    private sealed class ScreechDizzyOverlay : Overlay
    {
        private readonly ShaderInstance _shader;
        private readonly IGameTiming _timing;

        public float Intensity = 1f;

        public ScreechDizzyOverlay(IPrototypeManager proto, IGameTiming timing)
        {
            _timing = timing;
            _shader = proto.Index<ShaderPrototype>("ScreechDizzyShader").InstanceUnique();
        }

        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        public override bool RequestScreenTexture => true;

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null)
                return;

            _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _shader.SetParameter("time", (float)_timing.CurTime.TotalSeconds);
            _shader.SetParameter("intensity", Intensity);

            var handle = args.WorldHandle;

            handle.UseShader(_shader);
            handle.DrawRect(args.WorldBounds, Color.White);
            handle.UseShader(null);
        }
    }
}