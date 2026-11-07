using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Silencer;

public sealed class CCMSonicPulseOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<ShaderPrototype> WaveShader = "CCMSonicPulseWave";

    private readonly CCMSonicPulseVisualsSystem _visuals;

    private readonly ShaderInstance[] _shaders = new ShaderInstance[CCMSonicPulseVisualsSystem.MaxWaves];

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public CCMSonicPulseOverlay()
    {
        IoCManager.InjectDependencies(this);
        _visuals = _entity.System<CCMSonicPulseVisualsSystem>();

        for (var i = 0; i < _shaders.Length; i++)
        {
            _shaders[i] = _prototype.Index(WaveShader).InstanceUnique();
        }
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        foreach (var wave in _visuals.Waves)
        {
            if (wave.Origin.MapId == args.MapId)
                return true;
        }

        return false;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        var handle = args.WorldHandle;
        var viewport = args.Viewport;
        var size = (Vector2) viewport.Size;
        var index = 0;

        handle.SetTransform(Matrix3x2.Identity);

        foreach (var wave in _visuals.Waves)
        {
            if (wave.Origin.MapId != args.MapId || index >= _shaders.Length)
                continue;

            var radius = wave.Range + 1f;
            var box = Box2.CenteredAround(wave.Origin.Position, new Vector2(radius * 2, radius * 2));
            if (!box.Intersects(args.WorldAABB))
                continue;

            var center = ToScreenUv(viewport, size, wave.Origin.Position);
            var forward = ToScreenUv(viewport, size, wave.Origin.Position + wave.Direction);

            var shader = _shaders[index++];
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", center);
            shader.SetParameter("forward", forward);
            shader.SetParameter("range", wave.Range);
            shader.SetParameter("half_cone", (float) wave.Cone.Theta / 2f);
            shader.SetParameter("progress", _visuals.GetProgress(wave));

            handle.UseShader(shader);
            handle.DrawRect(box, Color.White);
        }

        handle.UseShader(null);
    }

    private static Vector2 ToScreenUv(IClydeViewport viewport, Vector2 size, Vector2 world)
    {
        var local = viewport.WorldToLocal(world);
        return new Vector2(local.X / size.X, 1f - local.Y / size.Y);
    }
}
