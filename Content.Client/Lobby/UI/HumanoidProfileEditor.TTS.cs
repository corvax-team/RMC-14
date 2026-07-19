// Forge TTS - integrates the TTS voice picker as an extra tab in the personalization editor.
using Content.Client._Forge.TTS;
using Content.Shared._Forge;

namespace Content.Client.Lobby.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private TTSTab? _ttsTab;

        private void RefreshTTS()
        {
            if (_cfgManager.GetCVar(ForgeVars.TTSEnabled))
            {
                if (_ttsTab != null)
                    return;

                _ttsTab = new TTSTab();
                TabContainer.AddChild(_ttsTab);
                TabContainer.SetTabTitle(TabContainer.ChildCount - 1, Loc.GetString("humanoid-profile-editor-voice-tab"));

                _ttsTab.OnVoiceSelected += OnTTSVoiceSelected;
                _ttsTab.OnPreviewRequested += OnTTSPreviewRequested;
            }
            else
            {
                if (_ttsTab == null)
                    return;

                TabContainer.RemoveChild(_ttsTab);
                _ttsTab.OnVoiceSelected -= OnTTSVoiceSelected;
                _ttsTab.OnPreviewRequested -= OnTTSPreviewRequested;
                _ttsTab.Dispose();
                _ttsTab = null;
            }
        }

        private void UpdateTTSControls()
        {
            RefreshTTS();

            if (Profile == null || _ttsTab == null)
                return;

            _ttsTab.UpdateControls(Profile, Profile.Sex);
        }

        private void OnTTSVoiceSelected(string voiceId)
        {
            if (Profile == null)
                return;

            Profile = Profile.WithVoice(voiceId);
            _ttsTab?.SetSelectedVoice(voiceId);
            SetDirty();
        }

        private void OnTTSPreviewRequested(string voiceId)
        {
            _entManager.System<TTSSystem>().RequestPreviewTTS(voiceId);
        }
    }
}
