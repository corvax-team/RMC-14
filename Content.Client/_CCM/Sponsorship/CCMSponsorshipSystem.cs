// CM14 rework: non-RMC edit marker.
using System;
using Content.Shared._CCM.Sponsorship;

namespace Content.Client._CCM.Sponsorship;

public sealed class CCMSponsorshipSystem : EntitySystem
{
    public event Action<CCMSponsorshipStatusSnapshot>? StatusReceived;

    public CCMSponsorshipStatusSnapshot? LatestStatus { get; private set; }

    public override void Initialize()
    {
        SubscribeNetworkEvent<CCMSponsorshipStatusResponseEvent>(OnStatusResponse);
    }

    public void RequestStatus()
    {
        RaiseNetworkEvent(new RequestCCMSponsorshipStatusEvent());
    }

    private void OnStatusResponse(CCMSponsorshipStatusResponseEvent ev)
    {
        LatestStatus = ev.Snapshot;
        StatusReceived?.Invoke(ev.Snapshot);
    }
}
