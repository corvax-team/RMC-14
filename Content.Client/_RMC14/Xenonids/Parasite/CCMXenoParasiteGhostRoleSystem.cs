using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._RMC14.Xenonids.Parasite;

public sealed class CCMXenoParasiteGhostRoleSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoParasiteComponent, GetVerbsEvent<InteractionVerb>>(OnParasiteGetVerbs);
        SubscribeLocalEvent<XenoParasiteThrowerComponent, GetVerbsEvent<InteractionVerb>>(OnCarrierGetVerbs);
        SubscribeLocalEvent<XenoEggComponent, GetVerbsEvent<InteractionVerb>>(OnEggGetVerbs);
    }

    private void OnParasiteGetVerbs(Entity<XenoParasiteComponent> parasite, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!HasComp<GhostComponent>(args.User))
            return;

        if (_mobState.IsDead(parasite.Owner) || HasComp<ParasiteSpentComponent>(parasite.Owner))
            return;

        var user = args.User;
        var isRoyal = HasComp<CCMRoyalParasiteComponent>(parasite.Owner);

        var verb = new InteractionVerb
        {
            Act = () =>
            {
                var evt = new CCMGhostTakeParasiteEvent(GetNetEntity(parasite.Owner), isRoyal);
                RaiseNetworkEvent(evt);
            },
            Text = isRoyal
                ? Loc.GetString("rmc-xeno-egg-royal-ghost-verb")
                : Loc.GetString("rmc-xeno-egg-ghost-verb"),
            Icon = new SpriteSpecifier.Texture(new ResPath(isRoyal
                ? "/Textures/_RMC14/Interface/VerbIcons/royalparasiteVerb.png"
                : "/Textures/_RMC14/Interface/VerbIcons/parasiteVerb.png")),
        };
        args.Verbs.Add(verb);
    }

    private void OnCarrierGetVerbs(Entity<XenoParasiteThrowerComponent> carrier, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!HasComp<GhostComponent>(args.User))
            return;

        var totalRegularParasites = carrier.Comp.CurParasites + carrier.Comp.CurParasitesInHands;
        var totalRoyalParasites = carrier.Comp.CurRoyalParasites + carrier.Comp.CurRoyalParasitesInHands;

        var availableRegularParasites = Math.Max(0, totalRegularParasites - carrier.Comp.ReservedParasites);
        var availableRoyalParasites = Math.Max(0, totalRoyalParasites - carrier.Comp.ReservedRoyalParasites);

        var user = args.User;
        var carrierOwner = carrier.Owner;

        if (availableRegularParasites > 0)
        {
            var verb = new InteractionVerb
            {
                Act = () =>
                {
                    var msg = new CCMGhostTakeCarrierParasiteEvent(GetNetEntity(carrierOwner), false);
                    RaiseNetworkEvent(msg);
                },
                Text = Loc.GetString("rmc-xeno-egg-ghost-verb"),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_RMC14/Interface/VerbIcons/parasiteVerb.png")),
                Priority = 1,
            };
            args.Verbs.Add(verb);
        }

        if (availableRoyalParasites > 0)
        {
            var verb = new InteractionVerb
            {
                Act = () =>
                {
                    var msg = new CCMGhostTakeCarrierParasiteEvent(GetNetEntity(carrierOwner), true);
                    RaiseNetworkEvent(msg);
                },
                Text = Loc.GetString("rmc-xeno-egg-royal-ghost-verb"),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_RMC14/Interface/VerbIcons/royalparasiteVerb.png")),
                Priority = 0,
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnEggGetVerbs(Entity<XenoEggComponent> egg, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!HasComp<GhostComponent>(args.User))
            return;

        if (egg.Comp.State != XenoEggState.Grown)
            return;

        if (TryComp<XenoFragileEggComponent>(egg, out var fragile) && fragile.SustainedBy != null)
            return;

        var isRoyalEgg = egg.Comp.Spawn == "CCMXenoRoyalParasite";
        var user = args.User;

        var verb = new InteractionVerb
        {
            Act = () =>
            {
                var evt = new CCMGhostTakeParasiteEvent(GetNetEntity(egg.Owner), isRoyalEgg);
                RaiseNetworkEvent(evt);
            },
            Text = isRoyalEgg
                ? Loc.GetString("rmc-xeno-egg-royal-ghost-verb")
                : Loc.GetString("rmc-xeno-egg-ghost-verb"),
            Icon = new SpriteSpecifier.Texture(new ResPath(isRoyalEgg
                ? "/Textures/_RMC14/Interface/VerbIcons/royalparasiteVerb.png"
                : "/Textures/_RMC14/Interface/VerbIcons/parasiteVerb.png")),
        };
        args.Verbs.Add(verb);
    }
}
