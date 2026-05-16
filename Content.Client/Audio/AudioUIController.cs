using Content.Shared.Audio;
using Content.Shared.CCVar;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Configuration;

namespace Content.Client.Audio;

public sealed class AudioUIController : UIController
{
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    private float _interfaceGain;
    private IAudioSource? _clickSource;
    private IAudioSource? _hoverSource;

    private const float ClickGain = 0.25f;
    private const float HoverGain = 0.05f;

    public override void Initialize()
    {
        base.Initialize();

        /*
         * This exists to load UI sounds outside of the game sim.
         */

        // No unsub coz never shuts down until program exit.
        _configManager.OnValueChanged(CCVars.UIClickSound, SetClickSound, true);
        _configManager.OnValueChanged(CCVars.UIHoverSound, SetHoverSound, true);

        _configManager.OnValueChanged(CCVars.InterfaceVolume, SetInterfaceVolume, true);
    }

    private void SetInterfaceVolume(float obj)
    {
        _interfaceGain = AudioHelpers.SanitizeGain(obj, CCVars.InterfaceVolume.DefaultValue);

        if (_clickSource != null)
        {
            _clickSource.Gain = AudioHelpers.SanitizeGain(ClickGain * _interfaceGain, 0f);
        }

        if (_hoverSource != null)
        {
            _hoverSource.Gain = AudioHelpers.SanitizeGain(HoverGain * _interfaceGain, 0f);
        }
    }

    private void SetClickSound(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var resource = GetSoundOrFallback(value, CCVars.UIClickSound.DefaultValue);
            if (resource == null)
            {
                _clickSource = null;
                UIManager.SetClickSound(null);
                return;
            }
            var source =
                _audioManager.CreateAudioSource(resource);

            if (source != null)
            {
                source.Gain = AudioHelpers.SanitizeGain(ClickGain * _interfaceGain, 0f);
                source.Global = true;
            }

            _clickSource = source;
            UIManager.SetClickSound(source);
        }
        else
        {
            _clickSource = null;
            UIManager.SetClickSound(null);
        }
    }

    private void SetHoverSound(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var hoverResource = GetSoundOrFallback(value, CCVars.UIHoverSound.DefaultValue);
            if (hoverResource == null)
            {
                _hoverSource = null;
                UIManager.SetHoverSound(null);
                return;
            }
            var hoverSource =
                _audioManager.CreateAudioSource(hoverResource);

            if (hoverSource != null)
            {
                hoverSource.Gain = AudioHelpers.SanitizeGain(HoverGain * _interfaceGain, 0f);
                hoverSource.Global = true;
            }

            _hoverSource = hoverSource;
            UIManager.SetHoverSound(hoverSource);
        }
        else
        {
            _hoverSource = null;
            UIManager.SetHoverSound(null);
        }
    }

    private AudioResource? GetSoundOrFallback(string path, string fallback)
    {
        if (_cache.TryGetResource(path, out AudioResource? resource))
            return resource;

        return _cache.TryGetResource(fallback, out AudioResource? fallbackResource)
            ? fallbackResource
            : null;
    }
}
