/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Client.Viewport;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._CE.ZLevels.Core;

public sealed class CEZLevelBlurOverlay : Overlay, IDisposable
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    public CEZLevelLowerFxMode Mode { get; set; } = CEZLevelLowerFxMode.Tint;
    private ShaderInstance? _blurShader;

    public override bool RequestScreenTexture => Mode == CEZLevelLowerFxMode.Blur;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ProtoId<ShaderPrototype> _zBlurShader = "CEZBlur";

    public CEZLevelBlurOverlay()
    {
        IoCManager.InjectDependencies(this);
        _blurShader = _proto.Index(_zBlurShader).InstanceUnique();
    }

    public new void Dispose()
    {
        _blurShader?.Dispose();
        _blurShader = null;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye is not ScalingViewport.ZEye zeye)
            return false;

        if (zeye.Depth >= 0)
            return false;

        if (args.MapId == MapId.Nullspace)
            return false;

        if (Mode == CEZLevelLowerFxMode.Off)
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var ambientColor = Color.Blue.WithAlpha(0.22f);

        if (_entity.TryGetComponent<MapLightComponent>(args.MapUid, out var mapLight))
        {
            ambientColor = mapLight.AmbientLightColor.WithAlpha(0.22f);
        }

        if (Mode == CEZLevelLowerFxMode.Tint)
        {
            args.WorldHandle.DrawRect(args.WorldBounds, ambientColor);
            return;
        }

        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        var blurColor = new Vector3(
            ambientColor.RByte,
            ambientColor.GByte,
            ambientColor.BByte);
        if (_entity.TryGetComponent<MapLightComponent>(args.MapUid, out var mapLight2))
        {
            blurColor = new Vector3(
                mapLight2.AmbientLightColor.R,
                mapLight2.AmbientLightColor.G,
                mapLight2.AmbientLightColor.B);
        }

        _blurShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _blurShader?.SetParameter("BLUR_COLOR", blurColor);

        var worldHandle = args.WorldHandle;
        worldHandle.UseShader(_blurShader);
        worldHandle.DrawRect(args.WorldBounds, Color.White);
        worldHandle.UseShader(null);
    }
}
