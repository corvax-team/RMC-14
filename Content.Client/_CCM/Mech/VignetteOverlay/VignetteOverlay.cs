using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Mech.Overlays;

public sealed class VignetteOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    
    private static readonly ProtoId<ShaderPrototype> Shader = "CCMVignetteMech";
    private readonly ShaderInstance _vignetteShader;

    public VignetteOverlay()
    {
        IoCManager.InjectDependencies(this);
        _vignetteShader = _prototypeManager.Index(Shader).InstanceUnique();
        
        _vignetteShader.SetParameter("intensity", 1.0f);
        _vignetteShader.SetParameter("transitionProgress", 0.0f);
        
        ZIndex = 10;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _vignetteShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        handle.UseShader(_vignetteShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    public void UpdateTransition(float progress)
    {
        _vignetteShader.SetParameter("transitionProgress", progress);
    }
}
