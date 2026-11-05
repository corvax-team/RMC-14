using Content.Server.Explosion.EntitySystems;
using Content.Shared._CCM.Silencer;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Server._CCM.Silencer;

/// <summary>
///     Blocks using Silencer gear (priming grenades etc.) without the Silencer skill.
/// </summary>
public sealed class CCMSilencerUseWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMSilencerUseWhitelistComponent, UseInHandEvent>(OnUseInHand,
            before: new[] { typeof(TriggerSystem) });
    }

    private void OnUseInHand(Entity<CCMSilencerUseWhitelistComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (_whitelist.IsValid(ent.Comp.Whitelist, args.User))
            return;

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString(ent.Comp.DenyPopup, ("item", ent.Owner)), args.User, args.User, PopupType.SmallCaution);
    }
}
