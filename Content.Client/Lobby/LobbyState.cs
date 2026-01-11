using Content.Client.GameTicking.Managers;
using Content.Client.Lobby.UI;
using Content.Client.UserInterface.Systems.Chat;
using Content.Client.TextScreen;
using Content.Client.Voting;
using Content.Shared.CCVar;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client.Lobby
{
    public sealed class LobbyState : State
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IVoteManager _voteManager = default!;

        private ClientGameTicker _gameTicker = default!;

        protected override Type? LinkedScreenType { get; } = typeof(LobbyGui);
        public LobbyGui? Lobby;

        protected override void Startup()
        {
            if (_userInterfaceManager.ActiveScreen == null)
            {
                return;
            }

            Lobby = (LobbyGui) _userInterfaceManager.ActiveScreen;

            var chatController = _userInterfaceManager.GetUIController<ChatUIController>();
            _gameTicker = _entityManager.System<ClientGameTicker>();

            chatController.SetMainChat(true);

            _voteManager.SetPopupContainer(Lobby.VoteContainer);
            LayoutContainer.SetAnchorPreset(Lobby, LayoutContainer.LayoutPreset.Wide);

            var width = _cfg.GetCVar(CCVars.ServerLobbyRightPanelWidth);
            Lobby.RightSide.SetWidth = width;

            UpdateLobbyUi();

            _gameTicker.InfoBlobUpdated += UpdateLobbyUi;
            _gameTicker.LobbyStatusUpdated += LobbyStatusUpdated;
        }

        protected override void Shutdown()
        {
            var chatController = _userInterfaceManager.GetUIController<ChatUIController>();
            chatController.SetMainChat(false);
            _gameTicker.InfoBlobUpdated -= UpdateLobbyUi;
            _gameTicker.LobbyStatusUpdated -= LobbyStatusUpdated;

            _voteManager.ClearPopupContainer();

            Lobby = null;
        }

        public void SwitchState(LobbyGui.LobbyGuiState state)
        {
            // Yeah I hate this but LobbyState contains all the badness for now.
            Lobby?.SwitchState(state);
        }

        public override void FrameUpdate(FrameEventArgs e)
        {
            UpdateRoundCountdown();
            if (_gameTicker.IsGameStarted)
            {
                var roundTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
                Lobby!.StationTime.Text = Loc.GetString("lobby-state-player-status-round-time", ("hours", roundTime.Hours), ("minutes", roundTime.Minutes));
                return;
            }

            Lobby!.StationTime.Text = Loc.GetString("lobby-state-player-status-round-not-started");
        }

        private void UpdateRoundCountdown()
        {
            if (Lobby == null)
                return;

            if (_gameTicker.IsGameStarted || _gameTicker.StartTime <= TimeSpan.Zero)
            {
                Lobby.RoundStartTimer.Visible = false;
                Lobby.RoundStartTimer.Text = string.Empty;
                return;
            }

            var timeLeft = _gameTicker.StartTime - _gameTiming.CurTime;
            if (timeLeft < TimeSpan.Zero)
                timeLeft = TimeSpan.Zero;

            var timeText = TextScreenSystem.TimeToString(timeLeft, getMilliseconds: false);
            Lobby.RoundStartTimer.Text = _gameTicker.Paused
                ? Loc.GetString("ui-lobby-round-start-paused")
                : Loc.GetString("ui-lobby-round-start-timer", ("time", timeText));
            Lobby.RoundStartTimer.Visible = true;
        }

        private void LobbyStatusUpdated()
        {
            UpdateLobbyBackground();
            UpdateLobbyUi();
        }

        private void UpdateLobbyUi()
        {
            if (_gameTicker.ServerInfoBlob != null)
            {
                Lobby!.ServerInfo.SetInfoBlob(_gameTicker.ServerInfoBlob);
            }

            Lobby!.SetReadyState(_gameTicker.AreWeReady);
            Lobby!.SetRoundState(_gameTicker.IsGameStarted);
        }

        private void UpdateLobbyBackground()
        {
            if (_gameTicker.LobbyBackground != null)
            {
                Lobby!.Background.Texture = _resourceCache.GetResource<TextureResource>(_gameTicker.LobbyBackground );
            }
            else
            {
                Lobby!.Background.Texture = null;
            }

        }

    }
}
