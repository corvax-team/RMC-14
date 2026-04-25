// CM14 rework: non-RMC edit marker.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._CCM.Sponsorship;
using Content.Shared.Preferences;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Sponsorship;

public sealed class CCMCustomizationManager : IPostInjectInit
{
    private readonly Dictionary<NetUserId, CCMCustomizationSnapshot> _cache = new();

    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly CCMSponsorshipManager _sponsorship = default!;

    public async Task<CCMCustomizationSnapshot> GetSnapshot(NetUserId userId)
    {
        if (_cache.TryGetValue(userId, out var cached))
        {
            // Sponsorship tier can change while the player is online (e.g. via `ccmsponsor`),
            // so re-normalize on access to avoid "locked forever" caches.
            cached = NormalizeSnapshot(userId, cached);
            _cache[userId] = cached;
            return cached;
        }

        cached = await _db.GetCCMCustomization(userId.UserId);
        cached = NormalizeSnapshot(userId, cached);
        _cache[userId] = cached;
        return cached;
    }

    public async Task<CCMCustomizationSnapshot> SaveSnapshot(NetUserId userId, CCMCustomizationSnapshot snapshot)
    {
        snapshot = NormalizeSnapshot(userId, snapshot);
        await _db.SaveCCMCustomization(userId.UserId, snapshot);
        _cache[userId] = snapshot;
        return snapshot;
    }

    public bool TryGetOocTagText(NetUserId userId, out string text)
    {
        text = string.Empty;

        if (!_cache.TryGetValue(userId, out var snapshot))
            return false;

        var tagId = string.IsNullOrWhiteSpace(snapshot.SelectedOocTagId)
            ? CCMOocTags.None
            : snapshot.SelectedOocTagId;

        if (tagId == CCMOocTags.Custom)
        {
            text = snapshot.CustomOocTagText.Trim();
            return text.Length > 0;
        }

        text = CCMOocTags.GetDisplayText(tagId);
        return text.Length > 0;
    }

    public bool TryGetChatColorHex(NetUserId userId, bool looc, out string colorHex)
    {
        colorHex = string.Empty;

        if (!_cache.TryGetValue(userId, out var snapshot))
            return false;

        var colorId = looc ? snapshot.SelectedLoocColorId : snapshot.SelectedOocColorId;
        if (string.IsNullOrWhiteSpace(colorId) || colorId == CCMChatColorPresets.Default)
            return false;

        colorHex = CCMChatColorPresets.GetHex(colorId);
        return colorHex.Length > 0;
    }

    public ArmorPreference GetArmorPreference(NetUserId userId)
    {
        if (!_cache.TryGetValue(userId, out var snapshot))
            return ArmorPreference.None;

        var selection = snapshot.Selections.FirstOrDefault(s => s.SlotId == "armor_variant");
        var valueId = selection?.ValueId ?? CCMCustomizationArmorVariantIds.None;

        return valueId switch
        {
            CCMCustomizationArmorVariantIds.Padded => ArmorPreference.Padded,
            CCMCustomizationArmorVariantIds.Padless => ArmorPreference.Padless,
            CCMCustomizationArmorVariantIds.Ridged => ArmorPreference.Ridged,
            CCMCustomizationArmorVariantIds.Carrier => ArmorPreference.Carrier,
            CCMCustomizationArmorVariantIds.Skull => ArmorPreference.Skull,
            CCMCustomizationArmorVariantIds.Smooth => ArmorPreference.Smooth,
            _ => ArmorPreference.None,
        };
    }

    private CCMCustomizationSnapshot NormalizeSnapshot(NetUserId userId, CCMCustomizationSnapshot snapshot)
    {
        var status = _sponsorship.GetStatus(userId);
        var customizationUnlocked = status.CustomizationUnlocked;

        var selections = new List<CCMCustomizationSelectionData>(snapshot.Selections.Length);
        foreach (var selection in snapshot.Selections)
        {
            var valueId = NormalizeSelectionValue(selection.SlotId, selection.ValueId, customizationUnlocked);
            selections.Add(new CCMCustomizationSelectionData(selection.SlotId, valueId));
        }

        var selectedTagId = NormalizeTagId(snapshot.SelectedOocTagId);
        var customTagText = string.Empty;
        if (selectedTagId == CCMOocTags.Custom && status.Tier >= CCMSponsorshipTier.SponsorIII)
        {
            customTagText = NormalizeCustomTag(snapshot.CustomOocTagText);
        }
        else if (selectedTagId == CCMOocTags.Custom)
        {
            selectedTagId = CCMOocTags.None;
        }

        var selectedOocColorId = NormalizeChatColorId(snapshot.SelectedOocColorId, status.Tier);
        var selectedLoocColorId = NormalizeChatColorId(snapshot.SelectedLoocColorId, status.Tier);

        return new CCMCustomizationSnapshot(
            selections.ToArray(),
            selectedTagId,
            customTagText,
            selectedOocColorId,
            selectedLoocColorId);
    }

    private static string NormalizeSelectionValue(string slotId, string valueId, bool customizationUnlocked)
    {
        if (!customizationUnlocked &&
            slotId is not "armor_palette" &&
            slotId is not "armor_variant" &&
            slotId is not "weapon_spray")
        {
            return "default";
        }

        return slotId switch
        {
            "xeno_defender" => valueId == "ccm_defender_skin" ? valueId : "default",
            "xeno_drone" => valueId == "ccm_drone_skin" ? valueId : "default",
            "xeno_queen" => valueId == "ccm_queen_skin" ? valueId : "default",
            "xeno_runner" => valueId == "ccm_runner_skin" ? valueId : "default",
            "xeno_sentinel" => valueId == "ccm_sentinel_skin" ? valueId : "default",
            "ghost" => valueId is
                "holo_green" or
                "holo_blue" or
                "holo_violet" or
                "holo_amber" or
                "holo_crimson" or
                "holo_teal" or
                "sponsor_pretor" or
                "sponsor_runi" or
                "sponsor_queen" or
                "sponsor_facehugger"
                ? valueId
                : "default",
            "weapon_spray" => valueId is
                CCMCustomizationCamouflageIds.Jungle or
                CCMCustomizationCamouflageIds.Desert or
                CCMCustomizationCamouflageIds.Snow or
                CCMCustomizationCamouflageIds.Classic or
                CCMCustomizationCamouflageIds.Urban
                    ? valueId
                    : CCMCustomizationCamouflageIds.Default,
            "armor_palette" => valueId is
                CCMCustomizationCamouflageIds.Jungle or
                CCMCustomizationCamouflageIds.Desert or
                CCMCustomizationCamouflageIds.Snow or
                CCMCustomizationCamouflageIds.Classic or
                CCMCustomizationCamouflageIds.Urban
                    ? valueId
                    : CCMCustomizationCamouflageIds.Default,
            "armor_variant" => valueId is
                CCMCustomizationArmorVariantIds.Padded or
                CCMCustomizationArmorVariantIds.Padless or
                CCMCustomizationArmorVariantIds.Ridged or
                CCMCustomizationArmorVariantIds.Carrier or
                CCMCustomizationArmorVariantIds.Skull or
                CCMCustomizationArmorVariantIds.Smooth
                    ? valueId
                    : CCMCustomizationArmorVariantIds.None,
            _ => "default",
        };
    }

    private static string NormalizeTagId(string? tagId)
    {
        if (string.IsNullOrWhiteSpace(tagId))
            return CCMOocTags.None;

        return tagId == CCMOocTags.Custom || CCMOocTags.IsValidPreset(tagId)
            ? tagId
            : CCMOocTags.None;
    }

    private static string NormalizeCustomTag(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sanitized = new string(text
            .Trim()
            .Where(c => c != '[' && c != ']' && c != '\n' && c != '\r' && c != '\t')
            .ToArray());

        if (sanitized.Length > CCMCustomizationConstants.CustomOocTagMaxLength)
            sanitized = sanitized[..CCMCustomizationConstants.CustomOocTagMaxLength];

        return sanitized;
    }

    private static string NormalizeChatColorId(string? colorId, CCMSponsorshipTier tier)
    {
        if (string.IsNullOrWhiteSpace(colorId))
            return CCMChatColorPresets.Default;

        if (!CCMChatColorPresets.IsValidPreset(colorId))
            return CCMChatColorPresets.Default;

        return CCMChatColorPresets.CanUsePreset(colorId, tier)
            ? colorId
            : CCMChatColorPresets.Default;
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var snapshot = await _db.GetCCMCustomization(session.UserId.UserId);
        _cache[session.UserId] = NormalizeSnapshot(session.UserId, snapshot);
        cancel.ThrowIfCancellationRequested();
    }

    private void ClientDisconnected(ICommonSession session)
    {
        _cache.Remove(session.UserId);
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
