using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server._RMC14.Xenonids.Parasite;

public sealed partial class XenoParasiteThrowerSystem
{
    private void InitializeDeath()
    {
        SubscribeLocalEvent<XenoParasiteThrowerComponent, MobStateChangedEvent>(OnParasiteCarrierStateChanged);
    }

    private void OnParasiteCarrierStateChanged(Entity<XenoParasiteThrowerComponent> carrier, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var basePosition = _transform.GetMapCoordinates(carrier);
        const float scatterRadius = 3f;

        var regularParasites = carrier.Comp.CurParasites - carrier.Comp.CurRoyalParasites;
        for (var i = 0; i < regularParasites; i++)
            ScatterParasite(carrier, false, GetScatterPosition(basePosition, scatterRadius));

        for (var i = 0; i < carrier.Comp.CurRoyalParasites; i++)
            ScatterParasite(carrier, true, GetScatterPosition(basePosition, scatterRadius));

        carrier.Comp.CurParasites = 0;
        carrier.Comp.CurRoyalParasites = 0;
        Dirty(carrier);
    }

    private MapCoordinates GetScatterPosition(MapCoordinates basePos, float radius)
    {
        var angle = _random.NextFloat() * MathF.PI * 2f;
        var distance = _random.NextFloat() * radius;
        var offset = new Vector2(
            MathF.Cos(angle) * distance,
            MathF.Sin(angle) * distance
        );
        return basePos.Offset(offset);
    }

    private void ScatterParasite(Entity<XenoParasiteThrowerComponent> carrier, bool isRoyal, MapCoordinates position)
    {
        var protoId = isRoyal ? "CCMXenoRoyalParasite" : "RMCXenoParasite";
        var parasite = Spawn(protoId, position);
    }
}

