// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Content.Client._CCM.Barks;
using Content.Shared._CCM.Barks;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;

namespace Content.Client.Lobby.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private readonly List<BarkPrototype> _barkVoiceOptions = new();
        private bool _syncingBarkControls;

        private void InitializeBarkSettings()
        {
            BarkSettingsContainer.Visible = _cfgManager.GetCVar(CCVars.BarksEnabled);
            if (!BarkSettingsContainer.Visible)
                return;
            RebuildBarkVoiceOptions();

            BarkVoiceButton.OnItemSelected += args =>
            {
                if (_syncingBarkControls || Profile == null)
                    return;

                if (args.Id < 0 || args.Id >= _barkVoiceOptions.Count)
                    return;

                BarkVoiceButton.SelectId(args.Id);
                var voice = _barkVoiceOptions[args.Id];
                Profile = Profile.WithBarkVoice(voice.ID);
                SetBarkVoiceTypeLabel(voice.VoiceType);
                SetDirty();
            };

            BarkPitchSlider.OnValueChanged += _ =>
            {
                if (_syncingBarkControls || Profile == null)
                    return;

                var pitch = Math.Clamp((float) BarkPitchSlider.Value, 0.7f, 1.4f);
                pitch = (float) Math.Round(pitch * 20f) / 20f;

                _syncingBarkControls = true;
                BarkPitchSlider.Value = pitch;
                BarkPitchValueLabel.Text = $"{pitch.ToString("0.00", CultureInfo.InvariantCulture)}x";
                _syncingBarkControls = false;

                Profile = Profile.WithBarkPitch(pitch);
                SetDirty();
            };

            BarkSpeedSlider.OnValueChanged += _ =>
            {
                if (_syncingBarkControls || Profile == null)
                    return;

                var speed = Math.Clamp((float) BarkSpeedSlider.Value, 0.7f, 1.4f);
                speed = (float) Math.Round(speed * 10f) / 10f;

                _syncingBarkControls = true;
                BarkSpeedSlider.Value = speed;
                BarkSpeedValueLabel.Text = $"{speed.ToString("0.0", CultureInfo.InvariantCulture)}x";
                _syncingBarkControls = false;

                Profile = Profile.WithBarkSpeed(speed);
                SetDirty();
            };

            BarkPreviewButton.OnPressed += _ =>
            {
                if (_barkVoiceOptions.Count == 0)
                    return;

                var selected = BarkVoiceButton.SelectedId;
                if (selected < 0 || selected >= _barkVoiceOptions.Count)
                    selected = 0;

                var voice = _barkVoiceOptions[selected];
                var pitch = Math.Clamp((float) BarkPitchSlider.Value, 0.7f, 1.4f);
                var speed = Math.Clamp((float) BarkSpeedSlider.Value, 0.7f, 1.4f);
                _entManager.System<BarkSystem>().RequestPreview(voice.ID, pitch, speed);
            };
        }

        private void UpdateBarkSettings()
        {
            BarkSettingsContainer.Visible = _cfgManager.GetCVar(CCVars.BarksEnabled);
            if (!BarkSettingsContainer.Visible)
                return;

            _syncingBarkControls = true;
            RebuildBarkVoiceOptions();

            if (_barkVoiceOptions.Count == 0)
            {
                BarkVoiceButton.Disabled = true;
                BarkPreviewButton.Disabled = true;
                BarkPitchSlider.Disabled = true;
                BarkSpeedSlider.Disabled = true;
                BarkVoiceTypeLabel.Text = Loc.GetString("ccm-humanoid-profile-editor-bark-type-normal");
                BarkPitchValueLabel.Text = "1.00x";
                BarkSpeedValueLabel.Text = "1.0x";
                _syncingBarkControls = false;
                return;
            }

            BarkVoiceButton.Disabled = Profile == null;
            BarkPreviewButton.Disabled = Profile == null;
            BarkPitchSlider.Disabled = Profile == null;
            BarkSpeedSlider.Disabled = Profile == null;

            var profileVoiceId = Profile?.BarkVoice ?? BarkPrototype.Default;
            var selected = _barkVoiceOptions.FindIndex(v => v.ID == profileVoiceId);
            if (selected < 0)
            {
                selected = 0;
                if (Profile != null)
                    Profile = Profile.WithBarkVoice(_barkVoiceOptions[selected].ID);
            }

            BarkVoiceButton.SelectId(selected);
            SetBarkVoiceTypeLabel(_barkVoiceOptions[selected].VoiceType);

            var pitch = Math.Clamp(Profile?.BarkPitch ?? 1f, 0.7f, 1.4f);
            BarkPitchSlider.Value = pitch;
            BarkPitchValueLabel.Text = $"{pitch.ToString("0.00", CultureInfo.InvariantCulture)}x";

            var speed = Math.Clamp(Profile?.BarkSpeed ?? 1f, 0.7f, 1.4f);
            BarkSpeedSlider.Value = speed;
            BarkSpeedValueLabel.Text = $"{speed.ToString("0.0", CultureInfo.InvariantCulture)}x";

            _syncingBarkControls = false;
        }

        private void SetBarkVoiceTypeLabel(BarkVoiceType voiceType)
        {
            var key = voiceType switch
            {
                BarkVoiceType.Normal => "ccm-humanoid-profile-editor-bark-type-normal",
                BarkVoiceType.Robot => "ccm-humanoid-profile-editor-bark-type-robot",
                BarkVoiceType.Alien => "ccm-humanoid-profile-editor-bark-type-alien",
                BarkVoiceType.Creature => "ccm-humanoid-profile-editor-bark-type-creature",
                _ => "ccm-humanoid-profile-editor-bark-type-normal"
            };

            BarkVoiceTypeLabel.Text = Loc.GetString(key);
        }

        private void RebuildBarkVoiceOptions()
        {
            var allRoundStart = _prototypeManager
                .EnumeratePrototypes<BarkPrototype>()
                .Where(proto => proto.RoundStart)
                .OrderBy(proto => Loc.GetString(proto.Name))
                .ToList();

            IEnumerable<BarkPrototype> filtered = allRoundStart;
            if (Profile != null)
            {
                filtered = Profile.Sex switch
                {
                    Sex.Male => allRoundStart.Where(static p =>
                        p.ID.StartsWith("BarkMale", StringComparison.Ordinal) ||
                        p.ID.StartsWith("BarkNeutral", StringComparison.Ordinal)),
                    Sex.Female => allRoundStart.Where(static p =>
                        p.ID.StartsWith("BarkFemale", StringComparison.Ordinal) ||
                        p.ID.StartsWith("BarkNeutral", StringComparison.Ordinal)),
                    _ => allRoundStart
                };
            }

            var filteredList = filtered.ToList();
            if (filteredList.Count == 0)
                filteredList = allRoundStart;

            BarkVoiceButton.Clear();
            _barkVoiceOptions.Clear();
            _barkVoiceOptions.AddRange(filteredList);

            for (var i = 0; i < _barkVoiceOptions.Count; i++)
            {
                var voice = _barkVoiceOptions[i];
                BarkVoiceButton.AddItem(Loc.GetString(voice.Name), i);
            }
        }
    }
}
