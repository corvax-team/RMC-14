// CM14 rework: non-RMC edit marker.
using System;
using Content.Shared._CCM.Sponsorship;

namespace Content.Client._CCM.Sponsorship;

public sealed class CCMCustomizationSystem : EntitySystem
{
    public event Action<CCMCustomizationSnapshot>? CustomizationReceived;

    public CCMCustomizationSnapshot? LatestSnapshot { get; private set; }

    public override void Initialize()
    {
        SubscribeNetworkEvent<CCMCustomizationResponseEvent>(OnCustomizationResponse);
    }

    public void RequestCustomization()
    {
        RaiseNetworkEvent(new RequestCCMCustomizationEvent());
    }

    public void SaveCustomization(CCMCustomizationSnapshot snapshot)
    {
        RaiseNetworkEvent(new SaveCCMCustomizationEvent(
            snapshot.Selections,
            snapshot.SelectedOocTagId,
            snapshot.CustomOocTagText,
            snapshot.SelectedOocColorId,
            snapshot.SelectedLoocColorId));
    }

    private void OnCustomizationResponse(CCMCustomizationResponseEvent ev)
    {
        LatestSnapshot = ev.Snapshot;
        CustomizationReceived?.Invoke(ev.Snapshot);
    }
}
