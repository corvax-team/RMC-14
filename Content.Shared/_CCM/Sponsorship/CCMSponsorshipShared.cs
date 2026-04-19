// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Sponsorship;

[Serializable, NetSerializable]
public enum CCMSponsorshipTier : byte
{
    None = 0,
    SponsorI,
    SponsorII,
    SponsorIII,
}

public static class CCMCustomizationConstants
{
    public const int CustomOocTagMaxLength = 12;
}

public static class CCMCustomizationCamouflageIds
{
    public const string Default = "default";
    public const string Jungle = "jungle";
    public const string Desert = "desert";
    public const string Snow = "snow";
    public const string Classic = "classic";
    public const string Urban = "urban";
}

public static class CCMCustomizationArmorVariantIds
{
    public const string None = "none";
    public const string Padded = "padded";
    public const string Padless = "padless";
    public const string Ridged = "ridged";
    public const string Carrier = "carrier";
    public const string Skull = "skull";
    public const string Smooth = "smooth";
}

public static class CCMChatColorPresets
{
    public const string Default = "default";

    public sealed record Preset(string DisplayKey, string Hex, CCMSponsorshipTier MinimumTier);

    public static readonly IReadOnlyDictionary<string, Preset> Presets = new Dictionary<string, Preset>
    {
        [Default] = new("ccm-customization-color-default", string.Empty, CCMSponsorshipTier.None),
        ["mint"] = new("ccm-customization-color-mint", "#6EFFB7", CCMSponsorshipTier.SponsorI),
        ["azure"] = new("ccm-customization-color-azure", "#77D7FF", CCMSponsorshipTier.SponsorI),
        ["amber"] = new("ccm-customization-color-amber", "#FFC766", CCMSponsorshipTier.SponsorII),
        ["rose"] = new("ccm-customization-color-rose", "#FF8CA8", CCMSponsorshipTier.SponsorII),
        ["violet"] = new("ccm-customization-color-violet", "#D88BFF", CCMSponsorshipTier.SponsorIII),
        ["crimson"] = new("ccm-customization-color-crimson", "#FF6D7A", CCMSponsorshipTier.SponsorIII),
    };

    public static bool IsValidPreset(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && Presets.ContainsKey(id);
    }

    public static bool CanUsePreset(string? id, CCMSponsorshipTier tier)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!Presets.TryGetValue(id, out var preset))
            return false;

        return tier >= preset.MinimumTier;
    }

    public static string GetHex(string id)
    {
        return Presets.TryGetValue(id, out var preset) ? preset.Hex : string.Empty;
    }

    public static string GetDisplayKey(string id)
    {
        return Presets.TryGetValue(id, out var preset) ? preset.DisplayKey : "ccm-customization-color-default";
    }
}

public static class CCMOocTags
{
    public const string None = "none";
    public const string Custom = "custom";

    public static readonly IReadOnlyDictionary<string, string> PresetDisplayTexts = new Dictionary<string, string>
    {
        [None] = string.Empty,
        ["predator"] = "Хищник",
        ["medic"] = "Медик",
        ["engineer"] = "Техник",
        ["veteran"] = "Ветеран",
        ["recon"] = "Разведка",
        ["assault"] = "Штурм",
        ["hive"] = "Улей",
    };

    public static bool IsValidPreset(string id)
    {
        return PresetDisplayTexts.ContainsKey(id);
    }

    public static string GetDisplayText(string id)
    {
        return PresetDisplayTexts.TryGetValue(id, out var value) ? value : string.Empty;
    }
}

[Serializable, NetSerializable]
public sealed class CCMSponsorshipStatusSnapshot
{
    public CCMSponsorshipTier Tier { get; }
    public string DonateUrl { get; }
    public long ExpirationUnixSeconds { get; }
    public string OocColorHex { get; }
    public string LoocColorHex { get; }
    public float RoleWeightBonus { get; }
    public bool QueueBypass { get; }
    public bool CustomizationUnlocked { get; }

    public CCMSponsorshipStatusSnapshot(
        CCMSponsorshipTier tier,
        string donateUrl,
        long expirationUnixSeconds,
        string oocColorHex,
        string loocColorHex,
        float roleWeightBonus,
        bool queueBypass,
        bool customizationUnlocked)
    {
        Tier = tier;
        DonateUrl = donateUrl;
        ExpirationUnixSeconds = expirationUnixSeconds;
        OocColorHex = oocColorHex;
        LoocColorHex = loocColorHex;
        RoleWeightBonus = roleWeightBonus;
        QueueBypass = queueBypass;
        CustomizationUnlocked = customizationUnlocked;
    }
}

[Serializable, NetSerializable]
public sealed class CCMCustomizationSelectionData
{
    public string SlotId { get; }
    public string ValueId { get; }

    public CCMCustomizationSelectionData(string slotId, string valueId)
    {
        SlotId = slotId;
        ValueId = valueId;
    }
}

[Serializable, NetSerializable]
public sealed class CCMCustomizationSnapshot
{
    public CCMCustomizationSelectionData[] Selections { get; }
    public string SelectedOocTagId { get; }
    public string CustomOocTagText { get; }
    public string SelectedOocColorId { get; }
    public string SelectedLoocColorId { get; }

    public CCMCustomizationSnapshot(
        CCMCustomizationSelectionData[] selections,
        string selectedOocTagId = CCMOocTags.None,
        string customOocTagText = "",
        string selectedOocColorId = CCMChatColorPresets.Default,
        string selectedLoocColorId = CCMChatColorPresets.Default)
    {
        Selections = selections;
        SelectedOocTagId = selectedOocTagId;
        CustomOocTagText = customOocTagText;
        SelectedOocColorId = selectedOocColorId;
        SelectedLoocColorId = selectedLoocColorId;
    }
}

[Serializable, NetSerializable]
public sealed class RequestCCMSponsorshipStatusEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class CCMSponsorshipStatusResponseEvent : EntityEventArgs
{
    public CCMSponsorshipStatusSnapshot Snapshot { get; }

    public CCMSponsorshipStatusResponseEvent(CCMSponsorshipStatusSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

[Serializable, NetSerializable]
public sealed class RequestCCMCustomizationEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class CCMCustomizationResponseEvent : EntityEventArgs
{
    public CCMCustomizationSnapshot Snapshot { get; }

    public CCMCustomizationResponseEvent(CCMCustomizationSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

[Serializable, NetSerializable]
public sealed class SaveCCMCustomizationEvent : EntityEventArgs
{
    public CCMCustomizationSelectionData[] Selections { get; }
    public string SelectedOocTagId { get; }
    public string CustomOocTagText { get; }
    public string SelectedOocColorId { get; }
    public string SelectedLoocColorId { get; }

    public SaveCCMCustomizationEvent(
        CCMCustomizationSelectionData[] selections,
        string selectedOocTagId,
        string customOocTagText,
        string selectedOocColorId,
        string selectedLoocColorId)
    {
        Selections = selections;
        SelectedOocTagId = selectedOocTagId;
        CustomOocTagText = customOocTagText;
        SelectedOocColorId = selectedOocColorId;
        SelectedLoocColorId = selectedLoocColorId;
    }
}
