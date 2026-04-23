namespace Content.Shared._CCM.Weapons.Ranged;

/// <summary>
/// Allows systems to override which entity is considered the shooter.
/// Handlers should set ShootingEntity and Handled = true to override.
/// </summary>
[ByRefEvent]
public record struct GetShootingEntityEvent(EntityUid? ShootingEntity, bool Handled);

/// <summary>
/// Allows systems to override the active weapon for an entity.
/// Handlers should set Weapon and Handled = true to override.
/// </summary>
[ByRefEvent]
public record struct GetActiveWeaponEvent(EntityUid? Weapon, bool Handled);
