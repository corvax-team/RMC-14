using System;
using System.Linq;
using System.Numerics;
using Content.Client._CCM.Sponsorship;
using Content.Client._RMC14.LinkAccount;
using Content.Client.Clothing;
using Content.Client.Guidebook;
using Content.Client.Humanoid;
using Content.Client.Inventory;
using Content.Client.Lobby.UI;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Station;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.Item;
using Content.Shared._CCM.Preferences;
using Content.Shared._CCM.Sponsorship;
using Content.Shared.CCVar;
using Content.Shared.Clothing;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Localizations;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Content.Shared._RMC14.CCVar;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Client.Lobby;

public sealed class LobbyUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IFileDialogManager _dialogManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly JobRequirementsManager _requirements = default!;
    [Dependency] private readonly MarkingManager _markings = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;
    [UISystemDependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [UISystemDependency] private readonly ClientInventorySystem _inventory = default!;
    [UISystemDependency] private readonly StationSpawningSystem _spawn = default!;
    [UISystemDependency] private readonly GuidebookSystem _guide = default!;
    private bool _characterSetupLayoutHooked;
    private Control? _characterSetupHost;
    private bool _pendingShowCharacterSetup;
    [UISystemDependency] private readonly CMArmorSystem _armorSystem = default!;
    [UISystemDependency] private readonly SharedItemSystem _item = default!;

    private CharacterSetupGui? _characterSetup;
    private HumanoidProfileEditor? _profileEditor;
    private CharacterSetupGuiSavePanel? _savePanel;
    private EntityUid? _lobbyHeaderPreviewDummy;
    private EntityUid? _oldLobbyPreviewDummy;
    private readonly Dictionary<int, Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo>> _jobPriorityChancesBySlot = new();
    private CCMCustomizationSystem? _ccmCustomizationSystem;
    private bool _ccmCustomizationSubscribed;

    /// <summary>
    /// This is the modified profile currently being edited.
    /// </summary>
    private HumanoidCharacterProfile? EditedProfile => _profileEditor?.Profile;

    private int? EditedSlot => _profileEditor?.CharacterSlot;

    private const string DefaultLobbyXenoPrefix = "XX";

    public override void Initialize()
    {
        base.Initialize();
        _prototypeManager.PrototypesReloaded += OnProtoReload;
        _preferencesManager.OnServerDataLoaded += PreferencesDataLoaded;
        _requirements.Updated += OnRequirementsUpdated;

        _configurationManager.OnValueChanged(CCVars.FlavorText, args =>
        {
            _profileEditor?.RefreshFlavorText();
        });

        _configurationManager.OnValueChanged(CCVars.GameRoleTimers, _ => RefreshProfileEditor());

        _configurationManager.OnValueChanged(CCVars.GameRoleWhitelist, _ => RefreshProfileEditor());

        _linkAccount.Updated += RefreshProfileEditor;
        _configurationManager.OnValueChanged(RMCCVars.RMCLobbyXenoName, _ => UpdateLobbyHeader());
        _configurationManager.OnValueChanged(RMCCVars.RMCLobbyUiStyle, _ => OnLobbyUiStyleChanged());
        // CCM rework lobby - start
        _contentLoc.CultureChanged += OnCultureChanged;
        // CCM rework lobby - end

    }

    private CCMCustomizationSystem? EnsureCustomizationSystem()
    {
        if (_ccmCustomizationSystem == null)
            _ccmCustomizationSystem = EntityManager.SystemOrNull<CCMCustomizationSystem>();

        if (_ccmCustomizationSystem != null && !_ccmCustomizationSubscribed)
        {
            _ccmCustomizationSystem.CustomizationReceived += OnCustomizationReceived;
            _ccmCustomizationSubscribed = true;
        }

        return _ccmCustomizationSystem;
    }


    private void OnRequirementsUpdated()
    {
        if (_profileEditor != null)
        {
            _profileEditor.RefreshAntags();
            _profileEditor.RefreshJobs();
        }
    }

    private void OnProtoReload(PrototypesReloadedEventArgs obj)
    {
        if (_profileEditor != null)
        {
            if (obj.WasModified<AntagPrototype>())
            {
                _profileEditor.RefreshAntags();
            }

            if (obj.WasModified<JobPrototype>() ||
                obj.WasModified<DepartmentPrototype>())
            {
                _profileEditor.RefreshJobs();
            }

            if (obj.WasModified<LoadoutPrototype>() ||
                obj.WasModified<LoadoutGroupPrototype>() ||
                obj.WasModified<RoleLoadoutPrototype>())
            {
                _profileEditor.RefreshLoadouts();
            }

            if (obj.WasModified<SpeciesPrototype>())
            {
                _profileEditor.RefreshSpecies();
            }

            if (obj.WasModified<TraitPrototype>())
            {
                _profileEditor.RefreshTraits();
            }
        }
    }

    private void PreferencesDataLoaded()
    {
        if (_stateManager.CurrentState is not LobbyState)
            return;

        EnsureCustomizationSystem()?.RequestCustomization();
        ReloadCharacterSetup();
        UpdateLobbyHeader();
    }

    public void OnStateEntered(LobbyState state)
    {
        EnsureCustomizationSystem()?.RequestCustomization();
        ReloadCharacterSetup();
        UpdateLobbyHeader();
    }

    public void OnStateExited(LobbyState state)
    {
        if (_ccmCustomizationSystem != null && _ccmCustomizationSubscribed)
        {
            _ccmCustomizationSystem.CustomizationReceived -= OnCustomizationReceived;
            _ccmCustomizationSubscribed = false;
            _ccmCustomizationSystem = null;
        }

        ClearLobbyHeaderPreview();
        _profileEditor?.Dispose();
        _characterSetup?.Dispose();

        _characterSetup = null;
        _profileEditor = null;
        _pendingShowCharacterSetup = false;
        if (_characterSetupHost != null)
        {
            _characterSetupHost.OnResized -= UpdateCharacterSetupLayout;
            _characterSetupHost = null;
        }
        _characterSetupLayoutHooked = false;
    }

    private void OnCustomizationReceived(CCMCustomizationSnapshot _)
    {
        if (_stateManager.CurrentState is not LobbyState)
            return;

        ReloadCharacterSetup();
        UpdateLobbyHeader();
    }

    private void OnLobbyUiStyleChanged()
    {
        if (_stateManager.CurrentState is not LobbyState lobby || lobby.Lobby == null || _characterSetup == null)
            return;

        var targetHost = lobby.Lobby.ActiveCharacterSetupState;
        if (_characterSetup.Parent != targetHost)
        {
            if (_characterSetupHost != null)
                _characterSetupHost.OnResized -= UpdateCharacterSetupLayout;

            _characterSetup.Orphan();
            targetHost.AddChild(_characterSetup);
            targetHost.OnResized += UpdateCharacterSetupLayout;
            _characterSetupHost = targetHost;
            _characterSetupLayoutHooked = true;
        }

        UpdateCharacterSetupLayout();
    }

    // CCM rework lobby - start
    private void OnCultureChanged(string _)
    {
        if (_stateManager.CurrentState is not LobbyState)
            return;

        ClearLobbyHeaderPreview();
        _profileEditor?.Dispose();
        _characterSetup?.Dispose();
        _profileEditor = null;
        _characterSetup = null;
        _pendingShowCharacterSetup = false;

        if (_characterSetupHost != null)
        {
            _characterSetupHost.OnResized -= UpdateCharacterSetupLayout;
            _characterSetupHost = null;
        }

        _characterSetupLayoutHooked = false;
        ReloadCharacterSetup();
    }
    // CCM rework lobby - end

    /// <summary>
    /// Reloads every single character setup control.
    /// </summary>
    public void ReloadCharacterSetup()
    {
        EnsureFallbackCharacter();

        HumanoidCharacterProfile? selectedProfile = null;
        int? selectedIndex = null;
        var prefs = _preferencesManager.Preferences;
        if (prefs != null && prefs.Characters.Count > 0)
        {
            selectedIndex = prefs.SelectedCharacterIndex;
            if (selectedIndex == null || !prefs.Characters.TryGetValue(selectedIndex.Value, out var profile))
            {
                var fallbackSlot = prefs.Characters.Keys.First();
                _preferencesManager.SelectCharacter(fallbackSlot);
                selectedIndex = fallbackSlot;
                profile = prefs.Characters[fallbackSlot];
            }

            selectedProfile = profile as HumanoidCharacterProfile;
        }

        var (characterGui, profileEditor) = EnsureGui();
        characterGui.Visible = false;
        profileEditor.Visible = false;
        UpdateCharacterSetupLayout();
        characterGui.ReloadCharacterPickers();
        profileEditor.SetProfile(selectedProfile, selectedIndex);
        ApplyJobPriorityChances();
        UpdateCharacterSetupLayout();
        _pendingShowCharacterSetup = true;
        UpdateLobbyHeader();
    }

    private void EnsureFallbackCharacter()
    {
        if (!_preferencesManager.ServerDataLoaded ||
            _preferencesManager.Preferences is not { Characters.Count: 0 } ||
            (_preferencesManager.Settings?.MaxCharacterSlots ?? 0) <= 0)
        {
            return;
        }

        _preferencesManager.CreateCharacter(HumanoidCharacterProfile.Random());
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_pendingShowCharacterSetup || _characterSetup == null || _profileEditor == null)
            return;

        if (_stateManager.CurrentState is not LobbyState)
            return;

        _characterSetup.Visible = true;
        _profileEditor.Visible = true;
        _characterSetup.InvalidateMeasure();
        _characterSetup.InvalidateArrange();
        _profileEditor.InvalidateMeasure();
        _profileEditor.InvalidateArrange();
        UpdateCharacterSetupLayout();
        _pendingShowCharacterSetup = false;
    }

    private void RefreshProfileEditor()
    {
        _profileEditor?.RefreshAntags();
        _profileEditor?.RefreshJobs();
        _profileEditor?.RefreshLoadouts();
        _profileEditor?.RefreshRMC(_linkAccount.Tier);
        ApplyJobPriorityChances();
    }

    private void SaveProfile()
    {
        DebugTools.Assert(EditedProfile != null);

        if (EditedProfile == null || EditedSlot == null)
            return;

        var selected = _preferencesManager.Preferences?.SelectedCharacterIndex;

        if (selected == null)
            return;

        _preferencesManager.UpdateCharacter(EditedProfile, EditedSlot.Value);
        ReloadCharacterSetup();
    }

    private void CloseProfileEditor()
    {
        if (_profileEditor == null)
            return;

        _profileEditor.SetProfile(null, null);
        _profileEditor.Visible = false;

        if (_stateManager.CurrentState is LobbyState lobbyGui)
        {
            lobbyGui.SwitchState(LobbyGui.LobbyGuiState.Default);
        }
    }

    public bool TryCloseCharacterSetupFromEscape()
    {
        if (_characterSetup == null || _profileEditor == null || !_characterSetup.Visible || !_profileEditor.Visible)
            return false;

        if (_profileEditor.Profile != null && _profileEditor.IsDirty)
        {
            OpenSavePanel();
            return true;
        }

        CloseProfileEditor();
        return true;
    }

    public void UpdateJobPriorityChances(int characterSlot, Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo> chances)
    {
        _jobPriorityChancesBySlot[characterSlot] = new Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo>(chances);
        ApplyJobPriorityChances();
    }

    private void OpenSavePanel()
    {
        if (_savePanel is { IsOpen: true })
            return;

        _savePanel = new CharacterSetupGuiSavePanel();

        _savePanel.SaveButton.OnPressed += _ =>
        {
            SaveProfile();

            _savePanel.Close();

            CloseProfileEditor();
        };

        _savePanel.NoSaveButton.OnPressed += _ =>
        {
            _savePanel.Close();

            CloseProfileEditor();
        };

        _savePanel.OpenCentered();
    }

    private (CharacterSetupGui, HumanoidProfileEditor) EnsureGui()
    {
        if (_characterSetup != null && _profileEditor != null)
        {
            _characterSetup.Visible = true;
            _profileEditor.Visible = true;
            return (_characterSetup, _profileEditor);
        }

        _profileEditor = new HumanoidProfileEditor(
            _preferencesManager,
            _configurationManager,
            EntityManager,
            _dialogManager,
            LogManager,
            _playerManager,
            _prototypeManager,
            _resourceCache,
            _requirements,
            _markings);

        _profileEditor.OnOpenGuidebook += _guide.OpenHelp;

        _characterSetup = new CharacterSetupGui(_profileEditor);
        _characterSetup.HorizontalExpand = false;
        _characterSetup.VerticalExpand = false;
        _characterSetup.Visible = false;
        LayoutContainer.SetAnchorPreset(_characterSetup, LayoutContainer.LayoutPreset.Center);

        _characterSetup.CloseRequested += () =>
        {
            // Open the save panel if we have unsaved changes.
            if (_profileEditor.Profile != null && _profileEditor.IsDirty)
            {
                OpenSavePanel();

                return;
            }

            // Reset sliders etc.
            CloseProfileEditor();
        };

        _profileEditor.Save += SaveProfile;

        _characterSetup.SelectCharacter += args =>
        {
            _preferencesManager.SelectCharacter(args);
            ReloadCharacterSetup();
        };

        _characterSetup.DeleteCharacter += args =>
        {
            _preferencesManager.DeleteCharacter(args);

            // Reload everything
            if (EditedSlot == args)
            {
                ReloadCharacterSetup();
            }
            else
            {
                // Only need to reload character pickers
                _characterSetup?.ReloadCharacterPickers();
            }
        };

        _characterSetup.StatsRequested += () =>
        {
            if (_stateManager.CurrentState is LobbyState { Lobby: { } lobbyGui })
                lobbyGui.OpenStats();
        };

        if (_stateManager.CurrentState is LobbyState lobby)
        {
            if (lobby.Lobby != null)
            {
                lobby.Lobby.ActiveCharacterSetupState.AddChild(_characterSetup);
                if (!_characterSetupLayoutHooked)
                {
                    lobby.Lobby.ActiveCharacterSetupState.OnResized += UpdateCharacterSetupLayout;
                    _characterSetupHost = lobby.Lobby.ActiveCharacterSetupState;
                    _characterSetupLayoutHooked = true;
                }
            }
        }

        return (_characterSetup, _profileEditor);
    }

    private const float CharacterSetupMinWidth = 1180f;
    private const float CharacterSetupMinHeight = 680f;
    private const float CharacterSetupDefaultMaxWidth = 1960f;
    private const float CharacterSetupDefaultMinOpenHeight = 840f;
    private const float CharacterSetupSmallScreenDefaultHeightFactor = 0.8f;
    private const float OldCharacterSetupMinWidth = 1600f;
    private const float OldCharacterSetupMinHeight = 860f;
    private const float OldCharacterSetupDefaultMinOpenHeight = 980f;
    private const float OldCharacterSetupSmallScreenDefaultHeightFactor = 0.9f;
    private const float OldCharacterSetupDefaultMaxWidth = 2200f;
    private static readonly Vector2 CharacterSetupViewportMargin = new(8f, 8f);

    private void UpdateCharacterSetupLayout()
    {
        if (_characterSetup == null)
            return;

        var parent = _characterSetup.Parent;
        var baseSize = Vector2.Zero;
        if (_stateManager.CurrentState is LobbyState lobby && lobby.Lobby != null)
            baseSize = lobby.Lobby.ActiveCharacterSetupState.Size;
        if (baseSize.X <= 1f || baseSize.Y <= 1f)
            baseSize = parent?.Size ?? Vector2.Zero;
        if (baseSize.X <= 1f || baseSize.Y <= 1f)
            return;

        var oldLobbyStyle = string.Equals(
            _configurationManager.GetCVar(RMCCVars.RMCLobbyUiStyle),
            "old",
            StringComparison.OrdinalIgnoreCase);
        var availableSize = new Vector2(
            MathF.Max(1f, baseSize.X - CharacterSetupViewportMargin.X * 2f),
            MathF.Max(1f, baseSize.Y - CharacterSetupViewportMargin.Y * 2f));
        var baseMinWidth = oldLobbyStyle ? OldCharacterSetupMinWidth : CharacterSetupMinWidth;
        var baseMinHeight = oldLobbyStyle ? OldCharacterSetupMinHeight : CharacterSetupMinHeight;
        var defaultMinOpenHeight = oldLobbyStyle ? OldCharacterSetupDefaultMinOpenHeight : CharacterSetupDefaultMinOpenHeight;
        var smallScreenHeightFactor = oldLobbyStyle ? OldCharacterSetupSmallScreenDefaultHeightFactor : CharacterSetupSmallScreenDefaultHeightFactor;
        var minWidth = MathF.Min(baseMinWidth, availableSize.X);
        var minHeight = MathF.Min(baseMinHeight, availableSize.Y);
        var preferredSize = _characterSetup.MeasurePreferredSize(availableSize);
        var defaultMaxWidth = oldLobbyStyle ? OldCharacterSetupDefaultMaxWidth : CharacterSetupDefaultMaxWidth;
        preferredSize = new Vector2(
            Math.Clamp(preferredSize.X, minWidth, MathF.Min(availableSize.X, defaultMaxWidth)),
            Math.Clamp(preferredSize.Y, minHeight, availableSize.Y));
        preferredSize.Y = Math.Clamp(
            MathF.Max(preferredSize.Y, MathF.Min(availableSize.Y, defaultMinOpenHeight)),
            minHeight,
            availableSize.Y);
        if (baseSize.Y <= 900f)
        {
            preferredSize.Y = Math.Clamp(
                MathF.Max(preferredSize.Y, availableSize.Y * smallScreenHeightFactor),
                minHeight,
                availableSize.Y);
        }

        _characterSetup.UpdateLayoutBounds(
            baseSize,
            new Vector2(minWidth, minHeight),
            availableSize);

        var size = _characterSetup.HasManualSize
            ? _characterSetup.ManualSize
            : preferredSize;
        size = new Vector2(
            Math.Clamp(size.X, minWidth, availableSize.X),
            Math.Clamp(size.Y, minHeight, availableSize.Y));

        var pos = (baseSize - size) / 2f;

        // CCM rework lobby - start
        if (_characterSetup.HasManualPosition)
        {
            var maxPos = new Vector2(
                MathF.Max(0f, baseSize.X - size.X),
                MathF.Max(0f, baseSize.Y - size.Y));
            pos = new Vector2(
                MathF.Min(MathF.Max(0f, _characterSetup.ManualPosition.X), maxPos.X),
                MathF.Min(MathF.Max(0f, _characterSetup.ManualPosition.Y), maxPos.Y));
        }
        // CCM rework lobby - end

        pos = new Vector2(
            MathF.Min(MathF.Max(0f, pos.X), MathF.Max(0f, baseSize.X - size.X)),
            MathF.Min(MathF.Max(0f, pos.Y), MathF.Max(0f, baseSize.Y - size.Y)));

        LayoutContainer.SetAnchorPreset(_characterSetup, LayoutContainer.LayoutPreset.TopLeft);
        _characterSetup.SetSize = size;
        LayoutContainer.SetPosition(_characterSetup, pos);
    }

    private void UpdateLobbyHeader()
    {
        if (_stateManager.CurrentState is not LobbyState lobby || lobby.Lobby == null)
            return;

        HumanoidCharacterProfile? profile = null;
        if (_preferencesManager.Preferences is { } prefs && prefs.TryGetSelectedCharacter(out var selectedCharacter))
            profile = selectedCharacter as HumanoidCharacterProfile;
        var characterName = string.IsNullOrWhiteSpace(profile?.Name)
            ? Loc.GetString("identity-unknown-name")
            : profile.Name;
        var formattedCharacterName = FormatLobbyCharacterName(characterName);
        lobby.Lobby.WelcomeCharacterName.Text = formattedCharacterName;
        UpdateLobbyNameFont(lobby.Lobby.WelcomeCharacterName, formattedCharacterName);
        lobby.Lobby.WelcomeXenoName.Text = BuildLobbyXenoName(profile);
        UpdateLobbyHeaderPreview(lobby, profile);
    }

    private void UpdateLobbyHeaderPreview(LobbyState lobby, HumanoidCharacterProfile? profile)
    {
        if (_lobbyHeaderPreviewDummy != null && EntityManager.EntityExists(_lobbyHeaderPreviewDummy.Value))
            EntityManager.DeleteEntity(_lobbyHeaderPreviewDummy.Value);

        if (_oldLobbyPreviewDummy != null && EntityManager.EntityExists(_oldLobbyPreviewDummy.Value))
            EntityManager.DeleteEntity(_oldLobbyPreviewDummy.Value);

        _lobbyHeaderPreviewDummy = LoadProfileEntity(profile, null, true);
        _oldLobbyPreviewDummy = LoadProfileEntity(profile, null, true);
        lobby.Lobby?.CenterCharacterSprite.SetEntity(_lobbyHeaderPreviewDummy);

        if (lobby.Lobby == null)
            return;

        var preview = lobby.Lobby.OldCharacterPreview;
        if (profile == null || _oldLobbyPreviewDummy == null)
        {
            preview.SetLoaded(false);
            preview.SetSummaryText(string.Empty);
            preview.ClearPreview();
            return;
        }

        preview.SetLoaded(true);
        preview.SetSummaryText(BuildOldLobbyPreviewSummary(profile));
        preview.SetSprite(_oldLobbyPreviewDummy.Value);
    }

    private void ClearLobbyHeaderPreview()
    {
        if (_lobbyHeaderPreviewDummy != null && EntityManager.EntityExists(_lobbyHeaderPreviewDummy.Value))
            EntityManager.DeleteEntity(_lobbyHeaderPreviewDummy.Value);

        if (_oldLobbyPreviewDummy != null && EntityManager.EntityExists(_oldLobbyPreviewDummy.Value))
            EntityManager.DeleteEntity(_oldLobbyPreviewDummy.Value);

        _lobbyHeaderPreviewDummy = null;
        _oldLobbyPreviewDummy = null;

        if (_stateManager.CurrentState is LobbyState lobby && lobby.Lobby != null)
        {
            lobby.Lobby.CenterCharacterSprite.SetEntity(null);
            lobby.Lobby.OldCharacterPreview.SetLoaded(false);
            lobby.Lobby.OldCharacterPreview.SetSummaryText(string.Empty);
            lobby.Lobby.OldCharacterPreview.ClearPreview();
        }
    }

    private string BuildOldLobbyPreviewSummary(HumanoidCharacterProfile profile)
    {
        var name = string.IsNullOrWhiteSpace(profile.Name)
            ? Loc.GetString("identity-unknown-name")
            : profile.Name;

        return Loc.GetString("ui-lobby-character-preview-summary",
            ("name", name),
            ("age", profile.Age));
    }

    private void UpdateLobbyNameFont(Label label, string? name)
    {
        var raw = name ?? string.Empty;
        var longestLine = raw.Split('\n').Max(static line => line.Length);
        var size = longestLine switch
        {
            <= 12 => 18,
            <= 21 => 15,
            _ => 14
        };

        label.FontOverride = _resourceCache.NotoStack(variation: "Bold", size: size);
    }

    private static string FormatLobbyCharacterName(string name)
    {
        if (name.Length <= 18)
            return name;

        var tokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length >= 2)
            return $"{tokens[0]}\n{string.Join(' ', tokens.Skip(1))}";

        var splitAt = Math.Clamp(name.Length / 2, 8, name.Length - 4);
        return $"{name[..splitAt]}\n{name[splitAt..]}";
    }

    private string BuildLobbyXenoName(HumanoidCharacterProfile? profile)
    {
        var fallback = EnsureLobbyXenoName();

        var prefix = profile?.XenoPrefix?.Trim() ?? string.Empty;
        var postfix = profile?.XenoPostfix?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(prefix) && string.IsNullOrWhiteSpace(postfix))
            return fallback;

        if (string.IsNullOrWhiteSpace(prefix))
            prefix = DefaultLobbyXenoPrefix;

        var number = ExtractLobbyXenoNumber(fallback);
        if (string.IsNullOrWhiteSpace(number))
            number = "000";

        return string.IsNullOrWhiteSpace(postfix)
            ? $"{prefix}-{number}"
            : $"{prefix}-{number}{postfix}";
    }

    private string EnsureLobbyXenoName()
    {
        var current = _configurationManager.GetCVar(RMCCVars.RMCLobbyXenoName);
        if (!string.IsNullOrWhiteSpace(current))
            return current;

        var generated = $"{DefaultLobbyXenoPrefix}-{_random.Next(0, 1000):000}";
        _configurationManager.SetCVar(RMCCVars.RMCLobbyXenoName, generated);
        return generated;
    }

    private static string ExtractLobbyXenoNumber(string name)
    {
        var dashIndex = name.IndexOf('-');
        if (dashIndex >= 0 && dashIndex + 1 < name.Length)
        {
            var digits = new string(name[(dashIndex + 1)..].TakeWhile(char.IsDigit).ToArray());
            if (digits.Length > 0)
                return digits.PadLeft(3, '0');
        }

        var allDigits = new string(name.Where(char.IsDigit).ToArray());
        if (allDigits.Length > 0)
        {
            if (allDigits.Length >= 3)
                return allDigits[..3];
            return allDigits.PadLeft(3, '0');
        }

        return string.Empty;
    }

    private void ApplyJobPriorityChances()
    {
        if (_profileEditor == null)
            return;

        var slot = _profileEditor.CharacterSlot;
        if (slot == null)
            return;

        var chances = _jobPriorityChancesBySlot.GetValueOrDefault(slot.Value) ??
            new Dictionary<ProtoId<JobPrototype>, JobPriorityChanceInfo>();

        _profileEditor.SetJobPriorityChances(slot.Value, chances);
    }

    #region Helpers

    /// <summary>
    /// Applies the highest priority job's clothes to the dummy.
    /// </summary>
    public void GiveDummyJobClothesLoadout(EntityUid dummy, JobPrototype? jobProto, HumanoidCharacterProfile profile)
    {
        var job = jobProto ?? GetPreferredJob(profile);
        GiveDummyJobClothes(dummy, profile, job);

        if (_prototypeManager.HasIndex<RoleLoadoutPrototype>(LoadoutSystem.GetJobPrototype(job.ID)))
        {
            var loadout = profile.GetLoadoutOrDefault(LoadoutSystem.GetJobPrototype(job.ID), _playerManager.LocalSession, profile.Species, EntityManager, _prototypeManager);
            GiveDummyLoadout(dummy, loadout);
        }
    }

    /// <summary>
    /// Gets the highest priority job for the profile.
    /// </summary>
    public JobPrototype GetPreferredJob(HumanoidCharacterProfile profile)
    {
        var firstPriorityJob = profile.JobPriorities.FirstOrDefault(p => p.Value.IsFirst()).Key;
        if (firstPriorityJob != default)
            return _prototypeManager.Index<JobPrototype>(firstPriorityJob.Id ?? SharedGameTicker.FallbackOverflowJob);

        var secondPriorityJob = profile.JobPriorities.FirstOrDefault(p => p.Value.IsSecond()).Key;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract (what is resharper smoking?)
        return _prototypeManager.Index<JobPrototype>(secondPriorityJob.Id ?? SharedGameTicker.FallbackOverflowJob);
    }

    public void GiveDummyLoadout(EntityUid uid, RoleLoadout? roleLoadout)
    {
        if (roleLoadout == null)
            return;

        foreach (var group in roleLoadout.SelectedLoadouts.Values)
        {
            foreach (var loadout in group)
            {
                if (!_prototypeManager.TryIndex(loadout.Prototype, out var loadoutProto))
                    continue;

                _spawn.EquipStartingGear(uid, loadoutProto);
            }
        }
    }

    /// <summary>
    /// Applies the specified job's clothes to the dummy.
    /// </summary>
    public void GiveDummyJobClothes(EntityUid dummy, HumanoidCharacterProfile profile, JobPrototype job)
    {
        var armorPreference = GetPreviewArmorPreference(profile);

        if (!_inventory.TryGetSlots(dummy, out var slots))
            return;

        // Apply loadout
        if (profile.Loadouts.TryGetValue(job.ID, out var jobLoadout))
        {
            foreach (var loadouts in jobLoadout.SelectedLoadouts.Values)
            {
                foreach (var loadout in loadouts)
                {
                    if (!_prototypeManager.TryIndex(loadout.Prototype, out var loadoutProto))
                        continue;

                    // TODO: Need some way to apply starting gear to an entity and replace existing stuff coz holy fucking shit dude.
                    foreach (var slot in slots)
                    {
                        // Try startinggear first
                        if (_prototypeManager.TryIndex(loadoutProto.StartingGear, out var loadoutGear))
                        {
                            var itemType = ((IEquipmentLoadout) loadoutGear).GetGear(slot.Name);

                            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
                            {
                                EntityManager.DeleteEntity(unequippedItem.Value);
                            }

                            if (itemType != string.Empty)
                            {
                                var item = EntityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);
                                _inventory.TryEquip(dummy, item, slot.Name, true, true);
                            }
                        }
                        else
                        {
                            var itemType = ((IEquipmentLoadout) loadoutProto).GetGear(slot.Name);

                            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
                            {
                                EntityManager.DeleteEntity(unequippedItem.Value);
                            }

                            if (itemType != string.Empty)
                            {
                                var item = EntityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);
                                _inventory.TryEquip(dummy, item, slot.Name, true, true);
                            }
                        }
                    }
                }
            }
        }

        if (!_prototypeManager.TryIndex(job.StartingGear, out var gear))
            return;

        _prototypeManager.TryIndex(job.DummyStartingGear, out var dummyGear);

        foreach (var slot in slots)
        {
            var itemType = ((IEquipmentLoadout) gear).GetGear(slot.Name);

            if (itemType == string.Empty && dummyGear != null)
                itemType = ((IEquipmentLoadout) dummyGear).GetGear(slot.Name);

            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
            {
                EntityManager.DeleteEntity(unequippedItem.Value);
            }

            if (itemType != string.Empty)
            {
                var item = EntityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);

                if (EntityManager.TryGetComponent<RMCArmorVariantComponent>(item, out var variantComponent))
                {
                    var variantItemProtoId = _armorSystem.GetArmorVariant((item, variantComponent), armorPreference);
                    var variantItem = EntityManager.SpawnEntity(variantItemProtoId, MapCoordinates.Nullspace);
                    _inventory.TryEquip(dummy, variantItem, slot.Name, true, true);
                    EntityManager.QueueDeleteEntity(item);

                    continue;
                }

                _inventory.TryEquip(dummy, item, slot.Name, true, true);
            }
        }
    }

    private ArmorPreference GetPreviewArmorPreference(HumanoidCharacterProfile profile)
    {
        var snapshot = EnsureCustomizationSystem()?.LatestSnapshot;
        if (snapshot == null)
            return profile.ArmorPreference;

        var selected = GetCustomizationSelection(snapshot, "armor_variant");
        return selected switch
        {
            CCMCustomizationArmorVariantIds.Padded => ArmorPreference.Padded,
            CCMCustomizationArmorVariantIds.Padless => ArmorPreference.Padless,
            CCMCustomizationArmorVariantIds.Ridged => ArmorPreference.Ridged,
            CCMCustomizationArmorVariantIds.Carrier => ArmorPreference.Carrier,
            CCMCustomizationArmorVariantIds.Skull => ArmorPreference.Skull,
            CCMCustomizationArmorVariantIds.Smooth => ArmorPreference.Smooth,
            CCMCustomizationArmorVariantIds.None => ArmorPreference.None,
            _ => profile.ArmorPreference,
        };
    }

    private void ApplyPreviewCustomization(EntityUid dummy)
    {
        var snapshot = EnsureCustomizationSystem()?.LatestSnapshot;
        if (snapshot == null)
            return;

        var appearanceSystem = EntityManager.System<AppearanceSystem>();
        var sharedAppearanceSystem = EntityManager.System<SharedAppearanceSystem>();

        ApplyWearablePreviewCamouflage(dummy, snapshot, sharedAppearanceSystem, appearanceSystem);
        ApplyHeldPreviewWeaponCamouflage(dummy, snapshot, sharedAppearanceSystem, appearanceSystem);
    }

    private void ApplyWearablePreviewCamouflage(
        EntityUid dummy,
        CCMCustomizationSnapshot snapshot,
        SharedAppearanceSystem sharedAppearanceSystem,
        AppearanceSystem appearanceSystem)
    {
        var camouflage = ParsePreviewCamouflage(GetCustomizationSelection(snapshot, "armor_palette"));
        var slots = new[] { "outerClothing", "jumpsuit", "head" };
        var changed = false;

        foreach (var slot in slots)
        {
            if (!_inventory.TryGetSlotEntity(dummy, slot, out var equipped) ||
                !EntityManager.TryGetComponent<ItemCamouflageComponent>(equipped, out _))
            {
                continue;
            }

            changed |= ApplyPreviewCamouflage(
                equipped.Value,
                camouflage,
                sharedAppearanceSystem,
                appearanceSystem,
                propagateVisuals: false);
        }

        if (changed && EntityManager.TryGetComponent<InventoryComponent>(dummy, out var inventory))
        {
            EntityManager.System<ClientClothingSystem>().InitClothing(dummy, inventory);
        }
    }

    private void ApplyHeldPreviewWeaponCamouflage(
        EntityUid dummy,
        CCMCustomizationSnapshot snapshot,
        SharedAppearanceSystem sharedAppearanceSystem,
        AppearanceSystem appearanceSystem)
    {
        var camouflage = ParsePreviewCamouflage(GetCustomizationSelection(snapshot, "weapon_spray"));

        foreach (var item in _inventory.GetHandOrInventoryEntities(dummy))
        {
            if (_inventory.TryGetContainingSlot(item, out _))
                continue;

            if (!EntityManager.HasComponent<ItemCamouflageComponent>(item))
                continue;

            ApplyPreviewCamouflage(
                item,
                camouflage,
                sharedAppearanceSystem,
                appearanceSystem,
                propagateVisuals: false);
        }
    }

    private bool ApplyPreviewCamouflage(
        EntityUid item,
        CamouflageType camouflage,
        SharedAppearanceSystem sharedAppearanceSystem,
        AppearanceSystem appearanceSystem,
        bool propagateVisuals)
    {
        if (!EntityManager.TryGetComponent<AppearanceComponent>(item, out var appearance))
            return false;

        EntityManager.TryGetComponent<SpriteComponent>(item, out var sprite);
        sharedAppearanceSystem.SetData(item, ItemCamouflageVisuals.Camo, camouflage);
        appearanceSystem.OnChangeData(item, sprite, appearance);

        if (propagateVisuals && EntityManager.HasComponent<ItemComponent>(item) && EntityManager.TryGetNetEntity(item, out _))
            _item.VisualsChanged(item);

        return true;
    }

    private static CamouflageType ParsePreviewCamouflage(string selected)
    {
        return selected switch
        {
            CCMCustomizationCamouflageIds.Desert => CamouflageType.Desert,
            CCMCustomizationCamouflageIds.Snow => CamouflageType.Snow,
            CCMCustomizationCamouflageIds.Classic => CamouflageType.Classic,
            CCMCustomizationCamouflageIds.Urban => CamouflageType.Urban,
            _ => CamouflageType.Jungle,
        };
    }

    private static string GetCustomizationSelection(CCMCustomizationSnapshot snapshot, string slotId)
    {
        foreach (var selection in snapshot.Selections)
        {
            if (selection.SlotId == slotId && !string.IsNullOrWhiteSpace(selection.ValueId))
                return selection.ValueId;
        }

        return "default";
    }

    /// <summary>
    /// Loads the profile onto a dummy entity.
    /// </summary>
    public EntityUid LoadProfileEntity(HumanoidCharacterProfile? humanoid, JobPrototype? job, bool jobClothes)
    {
        EntityUid dummyEnt;
        var effectiveJob = job;

        EntProtoId? previewEntity = null;
        if (humanoid != null && jobClothes && job != null)
        {
            previewEntity = job.JobPreviewEntity ?? (EntProtoId?)job?.JobEntity;
        }

        if (previewEntity != null)
        {
            // Special type like borg or AI, do not spawn a human just spawn the entity.
            dummyEnt = EntityManager.SpawnEntity(previewEntity, MapCoordinates.Nullspace);
            return dummyEnt;
        }
        else if (humanoid is not null)
        {
            var dummy = _prototypeManager.Index<SpeciesPrototype>(humanoid.Species).DollPrototype;
            dummyEnt = EntityManager.SpawnEntity(dummy, MapCoordinates.Nullspace);
        }
        else
        {
            dummyEnt = EntityManager.SpawnEntity(_prototypeManager.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies).DollPrototype, MapCoordinates.Nullspace);
        }

        _humanoid.LoadProfile(dummyEnt, humanoid);

        if (humanoid != null && jobClothes)
        {
            effectiveJob ??= GetPreferredJob(humanoid);
            if (effectiveJob == null)
                return dummyEnt;

            GiveDummyJobClothes(dummyEnt, humanoid, effectiveJob);

            if (_prototypeManager.HasIndex<RoleLoadoutPrototype>(LoadoutSystem.GetJobPrototype(effectiveJob.ID)))
            {
                var loadout = humanoid.GetLoadoutOrDefault(LoadoutSystem.GetJobPrototype(effectiveJob.ID), _playerManager.LocalSession, humanoid.Species, EntityManager, _prototypeManager);
                GiveDummyLoadout(dummyEnt, loadout);
            }

            ApplyPreviewCustomization(dummyEnt);
        }

        return dummyEnt;
    }

    #endregion
}

// # CCM priority rework
