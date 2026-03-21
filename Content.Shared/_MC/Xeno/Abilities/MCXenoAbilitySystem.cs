using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

// ReSharper disable UseCollectionExpression
namespace Content.Shared._MC.Xeno.Abilities;

public abstract class MCXenoAbilitySystem : EntitySystem
{
    [Dependency] protected readonly INetManager Net = null!;

    /// <summary>
    /// Reference to the central actions system used for validating and consuming ability actions.
    /// Automatically injected by dependency resolution.
    /// </summary>
    [Dependency] protected readonly SharedRMCActionsSystem RMCActions = null!;
    [Dependency] protected readonly SharedRMCMeleeWeaponSystem RMCMelee = null!;
    [Dependency] protected readonly SharedXenoHiveSystem RMCXenoHive = null!;

    [Dependency] protected readonly SharedActionsSystem Actions = null!;
    [Dependency] protected readonly SharedColorFlashEffectSystem ColorFlash = null!;
    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeapon = null!;

    [Dependency] private readonly MobStateSystem _mobState = null!;

public EntityUid ServerSpawnAttachedTo(string? prototype, EntityCoordinates coords, Angle rotation)
{
    if (Net.IsClient)
        return EntityUid.Invalid;

    var ent = SpawnAttachedTo(prototype, coords); 
    Transform(ent).LocalRotation = rotation;      
    return ent;
}
}