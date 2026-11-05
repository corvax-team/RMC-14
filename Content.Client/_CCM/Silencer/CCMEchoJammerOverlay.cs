using System.Numerics;
using Content.Shared._CCM.Silencer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Silencer;

public sealed class CCMEchoJammerOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<ShaderPrototype> WavesShader = "CCMEchoWaves";

    private readonly CCMEchoJammedVisualsSystem _visuals;
    private readonly TransformSystem _transform;

    private readonly Dictionary<EntityUid, ShaderInstance> _shaders = new();
    private readonly List<EntityUid> _stale = new();
    private readonly HashSet<EntityUid> _seen = new();

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public CCMEchoJammerOverlay()
    {
        IoCManager.InjectDependencies(this);

        _visuals = _entity.System<CCMEchoJammedVisualsSystem>();
        _transform = _entity.System<TransformSystem>();

        ZIndex = 10;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _visuals.LocalIsXenoWithNightVision();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var map = args.MapId;
        var handle = args.WorldHandle;
        _seen.Clear();

        handle.SetTransform(Matrix3x2.Identity);

        var query = _entity.EntityQueryEnumerator<CCMEchoJammerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var jammer, out var xform))
        {
            if (!jammer.Enabled)
                continue;

            var coords = _transform.GetMapCoordinates(xform);
            if (coords.MapId != map)
                continue;

            if (_visuals.IsVisibleFromEye(coords))
                continue;

            var length = MathF.Max(jammer.WaveMaxLength, 0.5f);
            var box = Box2.CenteredAround(coords.Position, new Vector2(length * 2, length * 2));
            if (!box.Intersects(args.WorldAABB))
                continue;

            _seen.Add(uid);
            if (!_shaders.TryGetValue(uid, out var shader))
            {
                shader = _prototype.Index(WavesShader).InstanceUnique();
                shader.SetParameter("seed", (uid.Id % 997) * 0.731f);
                _shaders[uid] = shader;
            }

            shader.SetParameter("max_length", length);

            handle.UseShader(shader);
            handle.DrawRect(box, Color.White);
        }

        handle.UseShader(null);

        _stale.Clear();
        foreach (var uid in _shaders.Keys)
        {
            if (!_seen.Contains(uid))
                _stale.Add(uid);
        }

        foreach (var uid in _stale)
        {
            _shaders.Remove(uid);
        }
    }
}
