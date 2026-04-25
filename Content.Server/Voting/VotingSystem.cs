// CM14 rework: non-RMC edit marker.
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Roles.Jobs;
using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Voting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Threading.Tasks;
using Content.Shared.Players.PlayTimeTracking;

namespace Content.Server.Voting;

public sealed class VotingSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ISharedPlaytimeManager _playtimeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<VotePlayerListRequestEvent>(OnVotePlayerListRequestEvent);
    }

    private async void OnVotePlayerListRequestEvent(VotePlayerListRequestEvent msg, EntitySessionEventArgs args)
    {
        // CCM-context change: votekick player list is disabled for ordinary clients.
        var deniedResponse = new VotePlayerListResponseEvent(Array.Empty<(NetUserId, NetEntity, string)>(), true);
        RaiseNetworkEvent(deniedResponse, args.SenderSession.Channel);
    }

    public string GetPlayerVoteListName(EntityUid attached)
    {
        TryComp<MindContainerComponent>(attached, out var mind);

        var jobName = _jobs.MindTryGetJobName(mind?.Mind);
        var playerInfo = $"{Comp<MetaDataComponent>(attached).EntityName} ({jobName})";

        return playerInfo;
    }

    /// <summary>
    /// Used to check whether the player initiating a votekick is allowed to do so serverside.
    /// </summary>
    /// <param name="initiator">The session initiating the votekick.</param>
    public async Task<bool> CheckVotekickInitEligibility(ICommonSession? initiator)
    {
        if (initiator == null)
            return false;

        // Being an admin overrides the votekick eligibility
        if (initiator.AttachedEntity != null && _adminManager.IsAdmin(initiator.AttachedEntity.Value, false))
            return true;

        // If cvar enabled, skip the ghost requirement in the preround lobby
        if (!_cfg.GetCVar(CCVars.VotekickIgnoreGhostReqInLobby) || (_cfg.GetCVar(CCVars.VotekickIgnoreGhostReqInLobby) && _gameTicker.RunLevel != GameRunLevel.PreRoundLobby))
        {
            if (_cfg.GetCVar(CCVars.VotekickInitiatorGhostRequirement))
            {
                // Must be ghost
                if (!TryComp(initiator.AttachedEntity, out GhostComponent? ghostComp))
                    return false;

                // Must have been dead for x seconds
                if ((int)_gameTiming.RealTime.Subtract(ghostComp.TimeOfDeath).TotalSeconds < _cfg.GetCVar(CCVars.VotekickEligibleVoterDeathtime))
                    return false;
            }
        }

        // Must be whitelisted
        if (!await _dbManager.GetWhitelistStatusAsync(initiator.UserId) && _cfg.GetCVar(CCVars.VotekickInitiatorWhitelistedRequirement))
            return false;

        // Must be eligible to vote
        var playtime = _playtimeManager.GetPlayTimes(initiator);
        return playtime.TryGetValue(PlayTimeTrackingShared.TrackerOverall, out TimeSpan overallTime) && (overallTime >= TimeSpan.FromHours(_cfg.GetCVar(CCVars.VotekickEligibleVoterPlaytime))
            || !_cfg.GetCVar(CCVars.VotekickInitiatorTimeRequirement));
    }

    /// <summary>
    /// Used to check whether the player being targetted for a votekick is a valid target.
    /// </summary>
    /// <param name="target">The session being targetted for a votekick.</param>
    public bool CheckVotekickTargetEligibility(ICommonSession? target)
    {
        if (target == null)
            return false;

        // Admins can't be votekicked
        if (target.AttachedEntity != null && _adminManager.IsAdmin(target.AttachedEntity.Value))
            return false;

        return true;
    }
}
