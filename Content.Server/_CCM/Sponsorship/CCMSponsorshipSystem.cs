// CM14 rework: non-RMC edit marker.
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.GameTicking;
using Content.Shared._CCM.Sponsorship;
using Robust.Server.Player;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Player;

namespace Content.Server._CCM.Sponsorship;

public sealed class CCMSponsorshipSystem : EntitySystem
{
    private const float SponsorshipRefreshIntervalSeconds = 60f;
    private float _sponsorshipRefreshAccumulator;
    private bool _refreshingSponsorships;

    [Dependency] private readonly CCMSponsorshipManager _sponsorship = default!;
    [Dependency] private readonly CCMCustomizationManager _customization = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestCCMSponsorshipStatusEvent>(OnRequestStatus);
        SubscribeNetworkEvent<RequestCCMCustomizationEvent>(OnRequestCustomization);
        SubscribeNetworkEvent<SaveCCMCustomizationEvent>(OnSaveCustomization);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    public void PushStatus(ICommonSession session)
    {
        RaiseNetworkEvent(new CCMSponsorshipStatusResponseEvent(_sponsorship.GetStatus(session.UserId)),
            session.Channel);
    }

    public async Task PushCustomization(ICommonSession session)
    {
        var snapshot = await _customization.GetSnapshot(session.UserId);
        if (session.AttachedEntity is { Valid: true } attached)
            EntityManager.System<CCMCustomizationApplySystem>().ApplyCustomization(attached, snapshot);

        RaiseNetworkEvent(new CCMCustomizationResponseEvent(snapshot), session.Channel);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _sponsorshipRefreshAccumulator += frameTime;
        if (_sponsorshipRefreshAccumulator < SponsorshipRefreshIntervalSeconds || _refreshingSponsorships)
            return;

        _sponsorshipRefreshAccumulator = 0f;
        _ = RefreshOnlineSponsorships();
    }

    private async Task RefreshOnlineSponsorships()
    {
        if (_refreshingSponsorships)
            return;

        _refreshingSponsorships = true;
        try
        {
            foreach (var session in _players.Sessions.ToArray())
            {
                var changed = await _sponsorship.RefreshSession(session, CancellationToken.None);
                if (!changed)
                    continue;

                PushStatus(session);
                await PushCustomization(session);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to refresh CCM sponsorship status:\n{e}");
        }
        finally
        {
            _refreshingSponsorships = false;
        }
    }

    private void OnRequestStatus(RequestCCMSponsorshipStatusEvent ev, EntitySessionEventArgs args)
    {
        PushStatus(args.SenderSession);
    }

    private async void OnRequestCustomization(RequestCCMCustomizationEvent ev, EntitySessionEventArgs args)
    {
        var snapshot = await _customization.GetSnapshot(args.SenderSession.UserId);
        RaiseNetworkEvent(new CCMCustomizationResponseEvent(snapshot), args.SenderSession.Channel);
    }

    private async void OnSaveCustomization(SaveCCMCustomizationEvent ev, EntitySessionEventArgs args)
    {
        var snapshot = await _customization.SaveSnapshot(
            args.SenderSession.UserId,
            new CCMCustomizationSnapshot(
                ev.Selections,
                ev.SelectedOocTagId,
                ev.CustomOocTagText,
                ev.SelectedOocColorId,
                ev.SelectedLoocColorId));

        if (args.SenderSession.AttachedEntity is { Valid: true } attached)
            EntityManager.System<CCMCustomizationApplySystem>().ApplyCustomization(attached, snapshot);

        RaiseNetworkEvent(new CCMCustomizationResponseEvent(snapshot), args.SenderSession.Channel);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var sponsors = _sponsorship.GetConnectedSponsorsForCredits();
        if (sponsors.Count == 0)
            return;

        var builder = new StringBuilder();
        builder.Append("[bold][color=#D9DDE3]");
        builder.Append(Loc.GetString("ccm-sponsorship-endgame-header"));
        builder.Append("[/color][/bold]\n");

        foreach (var (ckey, tier) in sponsors)
        {
            builder.Append("[color=");
            builder.Append(GetTierColor(tier));
            builder.Append(']');
            builder.Append(ckey);
            builder.Append("[/color] [color=#8E9AA8](");
            builder.Append(Loc.GetString(GetTierLocKey(tier)));
            builder.Append(")[/color]\n");
        }

        foreach (var line in builder.ToString().TrimEnd().Split('\n'))
        {
            ev.AddLine(line);
        }
    }

    private static string GetTierLocKey(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => "ccm-sponsorship-tier-3-title",
            CCMSponsorshipTier.SponsorII => "ccm-sponsorship-tier-2-title",
            _ => "ccm-sponsorship-tier-1-title",
        };
    }

    private static string GetTierColor(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => "#F6C453",
            CCMSponsorshipTier.SponsorII => "#D96CFF",
            _ => "#61C9FF",
        };
    }
}
