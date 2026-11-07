using Content.Shared._CCM.Silencer;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Silencer;

public sealed class CCMSonicCarbineVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly Color MuzzleLightColor = Color.FromHex("#7fe8ff");

    private CCMSonicCarbineOverlay _carbineOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMSonicImpactEffectComponent, ComponentStartup>(OnImpactStartup);
        SubscribeLocalEvent<CCMSonicMuzzleFlashComponent, ComponentStartup>(OnMuzzleStartup);

        _carbineOverlay = new CCMSonicCarbineOverlay();
        _overlay.AddOverlay(_carbineOverlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay(_carbineOverlay);
    }

    private void OnImpactStartup(Entity<CCMSonicImpactEffectComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Start = _timing.RealTime;
        ent.Comp.Seed = _random.NextFloat(0f, 100f);
    }

    private void OnMuzzleStartup(Entity<CCMSonicMuzzleFlashComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Start = _timing.RealTime;
        ent.Comp.Seed = _random.NextFloat(0f, 100f);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        // RMC GunSystem gives every muzzle flash an orange fire light; recolor it for the sonic flash.
        var query = EntityQueryEnumerator<CCMSonicMuzzleFlashComponent, PointLightComponent>();
        while (query.MoveNext(out var uid, out var flash, out var light))
        {
            if (flash.LightTinted)
                continue;

            flash.LightTinted = true;
            _lights.SetColor(uid, MuzzleLightColor, light);
            _lights.SetRadius(uid, 1.5f, light);
        }
    }

    public float GetProgress(CCMSonicImpactEffectComponent comp)
    {
        return GetProgress(comp.Start, comp.Duration);
    }

    public float GetProgress(CCMSonicMuzzleFlashComponent comp)
    {
        return GetProgress(comp.Start, comp.Duration);
    }

    private float GetProgress(TimeSpan start, float duration)
    {
        duration = MathF.Max(duration, 0.05f);
        var t = (float) ((_timing.RealTime - start).TotalSeconds / duration);
        return Math.Clamp(t, 0f, 1f);
    }
}
