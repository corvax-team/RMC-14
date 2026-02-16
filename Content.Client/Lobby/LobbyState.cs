using Content.Client.Audio;
using Content.Client.GameTicking.Managers;
using Content.Client.Lobby.UI;
using Content.Client.UserInterface.Systems.Chat;
using Content.Client.TextScreen;
using Content.Client.Voting;
using Content.Shared._RMC14.CCVar;
using Content.Shared.CCVar;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

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
        [Dependency] private readonly IRobustRandom _random = default!;

        private ClientGameTicker _gameTicker = default!;
        private ContentAudioSystem _audioSystem = default!;

        protected override Type? LinkedScreenType { get; } = typeof(LobbyGui);
        public LobbyGui? Lobby;
        private const float LobbyBackgroundRotationSeconds = 30f;
        private string? _activeLobbyBackgroundPreset;
        private string? _currentLobbyBackgroundPath;
        private IReadOnlyList<string>? _presetBackgrounds;
        private TimeSpan _nextLobbyBackgroundChange = TimeSpan.Zero;

        private static readonly Dictionary<string, string[]> LobbyBackgroundPresets = new()
        {
            {
                "console",
                new[]
                {
                    "/Textures/_CCM14/Lobby/lobbytgmc.png",
                    "/Textures/_CCM14/Lobby/lobbyweyland.png",
                }
            },
            {
                "community",
                new[]
                {
                    "/Textures/_CCM14/Lobby/CCM_logo_lobby.png",
                    "/Textures/_CCM14/Lobby/Letni_CCM.png",
                    "/Textures/_CCM14/Lobby/CCM_New.png",
                    "/Textures/_CCM14/Lobby/LobbyArt1.png",
                    "/Textures/_CCM14/Lobby/LobbyArt3.png",
                    "/Textures/_CCM14/Lobby/LobbyArt5.png",
                    "/Textures/_CCM14/Lobby/LobbyArt6.png",
                    "/Textures/_CCM14/Lobby/LobbyArt8.png",
                    "/Textures/_CCM14/Lobby/LobbyArt9.png",
                    "/Textures/_CCM14/Lobby/LobbyArt14.png",
                }
            },
            {
                "rmca",
                new[]
                {
                    "/Textures/_RMC14/Lobby/good_hits.png",
                    "/Textures/_RMC14/Lobby/running_from_ravager.png",
                    "/Textures/_RMC14/Lobby/Judgment_Day.png",
                    "/Textures/_RMC14/Lobby/WEYALobby.png",
                    "/Textures/_RMC14/Lobby/APushTooFar.png",
                    "/Textures/_RMC14/Lobby/LiquorCabinetsLastStand.png",
                    "/Textures/_RMC14/Lobby/Intelligence.png",
                    "/Textures/_RMC14/Lobby/Focus_Fire.png",
                    "/Textures/_RMC14/Lobby/Eyes_Up.png",
                    "/Textures/_RMC14/Lobby/Cornered.png",
                    "/Textures/_RMC14/Lobby/Move_Out.png",
                    "/Textures/_RMC14/Lobby/Lone_Medevac.png",
                    "/Textures/_RMC14/Lobby/Fly_the_Friendly_Skies.png",
                    "/Textures/_RMC14/Lobby/Battered.png",
                    "/Textures/_RMC14/Lobby/MarineMajor.png",
                    "/Textures/_RMC14/Lobby/smart_gun.png",
                    "/Textures/_RMC14/Lobby/sisters.png",
                    "/Textures/_RMC14/Lobby/only_you_shnee.png",
                    "/Textures/_RMC14/Lobby/from_fobbiton_with_love.png",
                    "/Textures/_RMC14/Lobby/PyrotechnicianXeno.png",
                }
            },
        };

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

            // CCM rework lobby - start
            var width = _cfg.GetCVar(CCVars.ServerLobbyRightPanelWidth) * 0.65f;
            var uiScale = Lobby.RightSide.UIScale;
            if (uiScale <= 0f)
                uiScale = 1f;
            var scaleAdjust = uiScale > 1f ? uiScale * 1.15f : MathF.Max(0.6f, uiScale * 0.9f);
            var desiredWidth = width / scaleAdjust;
            if (Lobby.Size.X > 1f)
                desiredWidth = MathF.Min(desiredWidth, Lobby.Size.X * 0.38f);
            Lobby.RightSide.SetWidth = desiredWidth;
            // CCM rework lobby - end

            UpdateLobbyUi();
            _cfg.OnValueChanged(RMCCVars.RMCLobbyBackgroundPreset, _ => UpdateLobbyBackground(true), true);

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
            UpdateLobbyBackgroundRotation();
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
            UpdateLobbyBackground(false);
        }

        private void UpdateLobbyBackground(bool force)
        {
            if (Lobby == null)
                return;

            var preset = _cfg.GetCVar(RMCCVars.RMCLobbyBackgroundPreset);
            var normalizedPreset = preset.ToLowerInvariant();
            if (!string.Equals(_activeLobbyBackgroundPreset, normalizedPreset, StringComparison.Ordinal))
            {
                _activeLobbyBackgroundPreset = normalizedPreset;
                force = true;
            }

            if (TryGetPresetBackgrounds(normalizedPreset, out var backgrounds))
            {
                _presetBackgrounds = backgrounds;
                if (force || _currentLobbyBackgroundPath == null)
                    SetLobbyBackground(PickRandomPresetBackground());
                ScheduleNextLobbyBackgroundChange();
                return;
            }

            _presetBackgrounds = null;
            var serverBackground = _gameTicker.LobbyBackground;
            if (force || !string.Equals(_currentLobbyBackgroundPath, serverBackground, StringComparison.OrdinalIgnoreCase))
                SetLobbyBackground(serverBackground);
        }

        private void UpdateLobbyBackgroundRotation()
        {
            if (Lobby == null || _presetBackgrounds == null)
                return;

            if (_gameTiming.CurTime < _nextLobbyBackgroundChange)
                return;

            SetLobbyBackground(PickRandomPresetBackground());
            ScheduleNextLobbyBackgroundChange();
        }

        private void ScheduleNextLobbyBackgroundChange()
        {
            _nextLobbyBackgroundChange = _gameTiming.CurTime + TimeSpan.FromSeconds(LobbyBackgroundRotationSeconds);
        }

        private string? PickRandomPresetBackground()
        {
            var backgrounds = _presetBackgrounds;
            if (backgrounds == null || backgrounds.Count == 0)
                return null;

            if (backgrounds.Count == 1)
                return backgrounds[0];

            var current = _currentLobbyBackgroundPath;
            var candidates = backgrounds.Where(path => !string.Equals(path, current, StringComparison.OrdinalIgnoreCase)).ToArray();
            return _random.Pick(candidates.Length > 0 ? candidates : backgrounds);
        }

        private void SetLobbyBackground(string? path)
        {
            if (Lobby == null)
                return;

            if (string.IsNullOrWhiteSpace(path))
            {
                Lobby.Background.Texture = null;
                _currentLobbyBackgroundPath = null;
                return;
            }

            Lobby.Background.Texture = _resourceCache.GetResource<TextureResource>(path);
            _currentLobbyBackgroundPath = path;
        }

        private static bool TryGetPresetBackgrounds(string preset, out string[] backgrounds)
        {
            if (LobbyBackgroundPresets.TryGetValue(preset, out var value))
            {
                backgrounds = value;
                return true;
            }

            backgrounds = Array.Empty<string>();
            return false;
        }

    }
}
