// CM14 rework: non-RMC edit marker.
using System;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._CCM.Sponsorship;

namespace Content.Server.Secrets.CCM.Sponsorship;

public sealed class CCMSponsorshipSecretsStubProvider : ICCMSponsorshipSecretsProvider
{
    public string DonateUrl => "https://example.com/donate";

    public ValueTask<CCMSponsorshipSecretsRecord> GetStatusAsync(Guid playerId, string ckey, CancellationToken cancel)
    {
        return ValueTask.FromResult(new CCMSponsorshipSecretsRecord(
            CCMSponsorshipTier.None,
            0,
            false,
            string.Empty,
            string.Empty));
    }
}
