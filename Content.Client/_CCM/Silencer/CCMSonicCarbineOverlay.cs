using System.Numerics;
using Content.Shared._CCM.Silencer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Silencer;

public sealed class CCMSonicCarbineOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<ShaderPrototype> BulletShader = "CCMSonicBullet";
    private static readonly ProtoId<ShaderPrototype> ImpactShader = "CCMSonicImpact";
    private static readonly ProtoId<ShaderPrototype> MuzzleShader = "CCMSonicMuzzle";

    private const int MaxBullets = 32;
    private const int MaxImpacts = 16;
    private const int MaxMuzzles = 8;

    private readonly CCMSonicCarbineVisualsSystem _visuals;
    private readonly TransformSystem _transform;
    private readonly SharedPhysicsSystem _physics;

    private readonly ShaderInstance[] _bulletShaders = new ShaderInstance[MaxBullets];
    private readonly ShaderInstance[] _impactShaders = new ShaderInstance[MaxImpacts];
    private readonly ShaderInstance[] _muzzleShaders = new ShaderInstance[MaxMuzzles];

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;
    public override bool RequestScreenTexture => true;

    public CCMSonicCarbineOverlay()
    {
        IoCManager.InjectDependencies(this);

        _visuals = _entity.System<CCMSonicCarbineVisualsSystem>();
        _transform = _entity.System<TransformSystem>();
        _physics = _entity.System<SharedPhysicsSystem>();

        for (var i = 0; i < MaxBullets; i++)
            _bulletShaders[i] = _prototype.Index(BulletShader).InstanceUnique();

        for (var i = 0; i < MaxImpacts; i++)
            _impactShaders[i] = _prototype.Index(ImpactShader).InstanceUnique();

        for (var i = 0; i < MaxMuzzles; i++)
            _muzzleShaders[i] = _prototype.Index(MuzzleShader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        return _entity.EntityQueryEnumerator<CCMSonicBulletVisualsComponent>().MoveNext(out _) ||
               _entity.EntityQueryEnumerator<CCMSonicImpactEffectComponent>().MoveNext(out _) ||
               _entity.EntityQueryEnumerator<CCMSonicMuzzleFlashComponent>().MoveNext(out _);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        var handle = args.WorldHandle;
        var viewport = args.Viewport;
        var size = (Vector2) viewport.Size;

        handle.SetTransform(Matrix3x2.Identity);

        var index = 0;
        var bullets = _entity.EntityQueryEnumerator<CCMSonicBulletVisualsComponent, SpriteComponent, TransformComponent>();
        while (bullets.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            if (index >= MaxBullets)
                break;

            // The bullet sprite is transparent, but RMC gun prediction still toggles its visibility
            // (hides the server copy for the shooter and spent bullets), so follow it.
            if (!sprite.Visible || xform.MapID != args.MapId)
                continue;

            var pos = _transform.GetWorldPosition(xform);
            var box = Box2.CenteredAround(pos, new Vector2(3f, 3f));
            if (!box.Intersects(args.WorldAABB))
                continue;

            var velocity = _physics.GetMapLinearVelocity(uid);
            var dir = velocity.LengthSquared() > 0.01f
                ? Vector2.Normalize(velocity)
                : _transform.GetWorldRotation(xform).ToWorldVec();

            var shader = _bulletShaders[index++];
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", ToScreenUv(viewport, size, pos));
            shader.SetParameter("forward", ToScreenUv(viewport, size, pos + dir));
            shader.SetParameter("seed", (uid.Id % 997) * 0.173f);

            handle.UseShader(shader);
            handle.DrawRect(box, Color.White);
        }

        index = 0;
        var impacts = _entity.EntityQueryEnumerator<CCMSonicImpactEffectComponent, TransformComponent>();
        while (impacts.MoveNext(out _, out var impact, out var xform))
        {
            if (index >= MaxImpacts)
                break;

            if (xform.MapID != args.MapId)
                continue;

            var pos = _transform.GetWorldPosition(xform);
            var box = Box2.CenteredAround(pos, new Vector2(3f, 3f));
            if (!box.Intersects(args.WorldAABB))
                continue;

            var shader = _impactShaders[index++];
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", ToScreenUv(viewport, size, pos));
            shader.SetParameter("unit", ToScreenUv(viewport, size, pos + new Vector2(1f, 0f)));
            shader.SetParameter("progress", _visuals.GetProgress(impact));
            shader.SetParameter("seed", impact.Seed);

            handle.UseShader(shader);
            handle.DrawRect(box, Color.White);
        }

        index = 0;
        var muzzles = _entity.EntityQueryEnumerator<CCMSonicMuzzleFlashComponent, TransformComponent>();
        while (muzzles.MoveNext(out _, out var flash, out var xform))
        {
            if (index >= MaxMuzzles)
                break;

            if (xform.MapID != args.MapId)
                continue;

            var (pos, rot) = _transform.GetWorldPositionRotation(xform);
            var dir = rot.ToVec();
            var muzzle = pos + dir * flash.MuzzleOffset;

            var box = Box2.CenteredAround(muzzle, new Vector2(2.5f, 2.5f));
            if (!box.Intersects(args.WorldAABB))
                continue;

            var shader = _muzzleShaders[index++];
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", ToScreenUv(viewport, size, muzzle));
            shader.SetParameter("forward", ToScreenUv(viewport, size, muzzle + dir));
            shader.SetParameter("progress", _visuals.GetProgress(flash));
            shader.SetParameter("seed", flash.Seed);

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
