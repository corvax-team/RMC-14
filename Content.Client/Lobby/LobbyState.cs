using Content.Client.Audio;
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
using System;
using System.Collections.Generic;

namespace Content.Client.Lobby
{
    public sealed class LobbyState : State
    {
        private readonly record struct LobbyTrackInfo(string Title, string Author);

        private static readonly Dictionary<string, LobbyTrackInfo> LobbyTrackInfoMap = new()
        {
            ["/audio/_rmc14/lobby/super_nova_in_the_catacombs.ogg"] = new("Super Nova In The Catacombs", "WigWoo1"),
            ["/audio/_rmc14/lobby/shadowinthesilvernebula.ogg"] = new("Shadow in the Silver Nebula", "Mendax"),
            ["/audio/_rmc14/lobby/the_fallen_queen.ogg"] = new("The Fallen Queen", "Bolgarich"),
            ["/audio/_rmc14/lobby/enemy_is_unknown.ogg"] = new("Enemy Is Unknown", "Nighty"),
            ["/audio/_rmc14/lobby/dire_situation.ogg"] = new("Dire Situation", "GoodShowOldChap"),
            ["/audio/_rmc14/lobby/time_is_running_out.ogg"] = new("Time Is Running Out", "Nighty"),
            ["/audio/_rmc14/lobby/dropzone.ogg"] = new("Dropzone", "Qwesta"),
        };

        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IVoteManager _voteManager = default!;

        private ClientGameTicker _gameTicker = default!;
        private ContentAudioSystem _audioSystem = default!;

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
            _audioSystem = _entityManager.System<ContentAudioSystem>();

            chatController.SetMainChat(true);

            _voteManager.SetPopupContainer(Lobby.VoteContainer);
            LayoutContainer.SetAnchorPreset(Lobby, LayoutContainer.LayoutPreset.Wide);

            var width = _cfg.GetCVar(CCVars.ServerLobbyRightPanelWidth);
            Lobby.RightSide.SetWidth = width;

            UpdateLobbyUi();

            _gameTicker.InfoBlobUpdated += UpdateLobbyUi;
            _gameTicker.LobbyStatusUpdated += LobbyStatusUpdated;

            _audioSystem.LobbySoundtrackChanged += UpdateLobbySoundtrackInfo;
            UpdateLobbySoundtrackInfo(new LobbySoundtrackChangedEvent(null));
        }

        protected override void Shutdown()
        {
            var chatController = _userInterfaceManager.GetUIController<ChatUIController>();
            chatController.SetMainChat(false);
            _gameTicker.InfoBlobUpdated -= UpdateLobbyUi;
            _gameTicker.LobbyStatusUpdated -= LobbyStatusUpdated;
            _audioSystem.LobbySoundtrackChanged -= UpdateLobbySoundtrackInfo;

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

        private void UpdateLobbySoundtrackInfo(LobbySoundtrackChangedEvent ev)
        {
            if (Lobby == null)
                return;

            if (ev.SoundtrackFilename == null)
            {
                Lobby.LobbyMusicText.Text = Loc.GetString("ui-lobby-music-none");
                return;
            }

            if (!TryGetLobbyTrackInfo(ev.SoundtrackFilename, out var track))
            {
                var title = GetTrackTitleFromFilename(ev.SoundtrackFilename);
                var author = Loc.GetString("ui-lobby-music-unknown");
                Lobby.LobbyMusicText.Text = FormatLobbyMusicLine(title, author);
                return;
            }

            Lobby.LobbyMusicText.Text = FormatLobbyMusicLine(track.Title, track.Author);
        }

        private static bool TryGetLobbyTrackInfo(string filename, out LobbyTrackInfo info)
        {
            return LobbyTrackInfoMap.TryGetValue(filename.ToLowerInvariant(), out info);
        }

        private static string GetTrackTitleFromFilename(string filename)
        {
            var start = filename.LastIndexOf('/') + 1;
            if (start < 0)
                start = 0;
            var end = filename.LastIndexOf('.');
            if (end <= start)
                end = filename.Length;
            var name = filename.Substring(start, end - start);
            return name.Replace('_', ' ');
        }

        private static string FormatLobbyMusicLine(string title, string author)
        {
            var line = Loc.GetString("ui-lobby-music-line", ("title", title), ("author", author));
            if (line.Length <= 36 && title.Length <= 24 && author.Length <= 20)
                return line;

            return $"{title}{Environment.NewLine}— {author}";
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
