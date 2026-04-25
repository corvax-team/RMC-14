// CM14 rework: non-RMC edit marker.
using System;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._CCM.Sponsorship;

namespace Content.Server.Secrets.CCM.Sponsorship;

public interface ICCMSponsorshipSecretsProvider
{
    string DonateUrl { get; }

    ValueTask<CCMSponsorshipSecretsRecord> GetStatusAsync(Guid playerId, string ckey, CancellationToken cancel);
}

public sealed record CCMSponsorshipSecretsRecord(
    CCMSponsorshipTier Tier,
    long ExpirationUnixSeconds,
    bool QueueBypass,
    string OocColorHex,
    string LoocColorHex);
