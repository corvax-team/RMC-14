// CM14 rework: non-RMC edit marker.
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Shared._CCM.Sponsorship;
using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.GhostColor;
using Content.Shared._RMC14.Item;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Strain;
using Content.Shared.Ghost;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Sponsorship;

public sealed class CCMCustomizationApplySystem : EntitySystem
{
    private static readonly string[] WearableCamouflageSlots =
    [
        "outerClothing",
        "jumpsuit",
        "head",
    ];

    private static readonly Dictionary<string, Color?> GhostColors = new()
    {
        ["default"] = null,
        ["holo_green"] = Color.FromHex("#7CFF9A88"),
        ["holo_blue"] = Color.FromHex("#77E3FF88"),
        ["holo_violet"] = Color.FromHex("#C695FF88"),
        ["holo_amber"] = Color.FromHex("#FFC76A88"),
        ["holo_crimson"] = Color.FromHex("#FF7C9C88"),
        ["holo_teal"] = Color.FromHex("#6FF2E888"),
    };

    private static readonly Dictionary<string, string> GhostSkinPaths = new()
    {
        ["sponsor_pretor"] = "_CCM14/Mobs/Ghost/sponsorGhostPretor.rsi",
        ["sponsor_runi"] = "_CCM14/Mobs/Ghost/sponsorGhostRuni.rsi",
        ["sponsor_queen"] = "_CCM14/Mobs/Ghost/sponsorGhostQueen.rsi",
        ["sponsor_facehugger"] = "_CCM14/Mobs/Ghost/sponsorGhostFacehugger.rsi",
    };

    private static readonly Dictionary<string, string> XenoSkinPaths = new()
    {
        ["xeno_defender:ccm_defender_skin"] = "_CCM14/Mobs/Xenonids/Skins/Defender",
        ["xeno_drone:ccm_drone_skin"] = "_CCM14/Mobs/Xenonids/Skins/Drone",
        ["xeno_queen:ccm_queen_skin"] = "_CCM14/Mobs/Xenonids/Skins/Queen",
        ["xeno_runner:ccm_runner_skin"] = "_CCM14/Mobs/Xenonids/Skins/Runner",
        ["xeno_sentinel:ccm_sentinel_skin"] = "_CCM14/Mobs/Xenonids/Skins/Sentinel",
    };

    [Dependency] private readonly CCMCustomizationManager _customization = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemCamouflageSystem _camouflage = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ActorComponent, RMCArmorVariantCreatedEvent>(OnArmorVariantCreated);
        SubscribeLocalEvent<ItemCamouflageComponent, GotEquippedEvent>(OnItemGotEquipped);
        SubscribeLocalEvent<ItemCamouflageComponent, GotEquippedHandEvent>(OnItemGotEquippedHand);
    }

    public void ApplyCustomization(EntityUid entity, CCMCustomizationSnapshot snapshot)
    {
        if (HasComp<GhostComponent>(entity))
            ApplyGhostCustomization(entity, snapshot);

        if (TryComp<XenoComponent>(entity, out var xeno))
            ApplyXenoCustomization((entity, xeno), snapshot);

        ApplyEquippedWearableCamouflage(entity, snapshot);
        ApplyHeldWeaponCamouflage(entity, snapshot);
    }

    private async void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var snapshot = await _customization.GetSnapshot(ev.Player.UserId);
        ApplyCustomization(ev.Entity, snapshot);
    }

    private void OnArmorVariantCreated(Entity<ActorComponent> ent, ref RMCArmorVariantCreatedEvent args)
    {
        if (TryComp<ItemCamouflageComponent>(args.New, out var camouflage))
            _ = ApplyArmorCamouflageAsync((args.New, camouflage), ent.Comp.PlayerSession.UserId);
    }

    private void OnItemGotEquipped(Entity<ItemCamouflageComponent> item, ref GotEquippedEvent args)
    {
        if (!TryComp<ActorComponent>(args.Equipee, out var actor))
        {
            return;
        }

        if (HasComp<GunComponent>(item))
            return;

        if (SupportsWearableCamouflage(args.Slot))
            _ = ApplyArmorCamouflageAsync(item, actor.PlayerSession.UserId);
    }

    private void OnItemGotEquippedHand(Entity<ItemCamouflageComponent> item, ref GotEquippedHandEvent args)
    {
        if (!HasComp<GunComponent>(item) ||
            !TryComp<ActorComponent>(args.User, out var actor))
        {
            return;
        }

        _ = ApplyWeaponCamouflageAsync(item, actor.PlayerSession.UserId);
    }

    private async Task ApplyArmorCamouflageAsync(Entity<ItemCamouflageComponent> item, NetUserId userId)
    {
        var snapshot = await _customization.GetSnapshot(userId);
        ApplyArmorCamouflage(item, snapshot);
    }

    private async Task ApplyWeaponCamouflageAsync(Entity<ItemCamouflageComponent> item, NetUserId userId)
    {
        var snapshot = await _customization.GetSnapshot(userId);
        ApplyWeaponCamouflage(item, snapshot);
    }

    private void ApplyGhostCustomization(EntityUid ghost, CCMCustomizationSnapshot snapshot)
    {
        var selected = GetSelection(snapshot, "ghost");
        var component = EnsureComp<GhostColorComponent>(ghost);
        GhostColors.TryGetValue(selected, out var color);
        component.Color = color;
        component.RsiPath = GhostSkinPaths.GetValueOrDefault(selected, string.Empty);
        Dirty(ghost, component);
    }

    private void ApplyXenoCustomization(Entity<XenoComponent> xeno, CCMCustomizationSnapshot snapshot)
    {
        if (HasComp<XenoStrainComponent>(xeno))
        {
            RemCompDeferred<CCMXenoSkinComponent>(xeno);
            return;
        }

        var slotId = xeno.Comp.Role.Id switch
        {
            "CMXenoDefender" => "xeno_defender",
            "CMXenoDrone" => "xeno_drone",
            "CMXenoQueen" => "xeno_queen",
            "CMXenoRunner" => "xeno_runner",
            "CMXenoSentinel" => "xeno_sentinel",
            _ => null,
        };

        if (slotId == null)
        {
            RemCompDeferred<CCMXenoSkinComponent>(xeno);
            return;
        }

        var selected = GetSelection(snapshot, slotId);
        if (!XenoSkinPaths.TryGetValue($"{slotId}:{selected}", out var rsiPath))
        {
            RemCompDeferred<CCMXenoSkinComponent>(xeno);
            return;
        }

        var component = EnsureComp<CCMXenoSkinComponent>(xeno);
        component.RsiPath = rsiPath;
        Dirty(xeno, component);
    }

    private void ApplyEquippedWearableCamouflage(EntityUid player, CCMCustomizationSnapshot snapshot)
    {
        if (!HasComp<MarineComponent>(player))
        {
            return;
        }

        foreach (var slot in WearableCamouflageSlots)
        {
            if (!_inventory.TryGetSlotEntity(player, slot, out var equipped) ||
                !TryComp<ItemCamouflageComponent>(equipped, out var camouflage))
            {
                continue;
            }

            ApplyArmorCamouflage((equipped.Value, camouflage), snapshot);
        }
    }

    private void ApplyHeldWeaponCamouflage(EntityUid player, CCMCustomizationSnapshot snapshot)
    {
        foreach (var item in _inventory.GetHandOrInventoryEntities(player))
        {
            if (!TryComp<ItemCamouflageComponent>(item, out var camouflage) ||
                !HasComp<GunComponent>(item))
            {
                continue;
            }

            ApplyWeaponCamouflage((item, camouflage), snapshot);
        }
    }

    private void ApplyArmorCamouflage(Entity<ItemCamouflageComponent> item, CCMCustomizationSnapshot snapshot)
    {
        SetItemCamouflage(item, ParseCamouflageSelection(GetSelection(snapshot, "armor_palette")));
    }

    private void ApplyWeaponCamouflage(Entity<ItemCamouflageComponent> item, CCMCustomizationSnapshot snapshot)
    {
        SetItemCamouflage(item, ParseCamouflageSelection(GetSelection(snapshot, "weapon_spray")));
    }

    private void SetItemCamouflage(Entity<ItemCamouflageComponent> item, CamouflageType camouflage)
    {
        EnsureComp<AppearanceComponent>(item);
        _appearance.SetData(item, ItemCamouflageVisuals.Camo, camouflage);
        _item.VisualsChanged(item);
    }

    private CamouflageType ParseCamouflageSelection(string selected)
    {
        return selected switch
        {
            CCMCustomizationCamouflageIds.Desert => CamouflageType.Desert,
            CCMCustomizationCamouflageIds.Snow => CamouflageType.Snow,
            CCMCustomizationCamouflageIds.Classic => CamouflageType.Classic,
            CCMCustomizationCamouflageIds.Urban => CamouflageType.Urban,
            CCMCustomizationCamouflageIds.Default => _camouflage.CurrentMapCamouflage,
            _ => _camouflage.CurrentMapCamouflage,
        };
    }

    private static string GetSelection(CCMCustomizationSnapshot snapshot, string slotId)
    {
        foreach (var selection in snapshot.Selections)
        {
            if (selection.SlotId == slotId && !string.IsNullOrWhiteSpace(selection.ValueId))
                return selection.ValueId;
        }

        return "default";
    }

    private static bool SupportsWearableCamouflage(string slot)
    {
        foreach (var candidate in WearableCamouflageSlots)
        {
            if (candidate == slot)
                return true;
        }

        return false;
    }
}
