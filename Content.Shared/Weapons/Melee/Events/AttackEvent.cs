using Content.Shared.Damage;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Weapons.Melee.Events
{
    [Serializable, NetSerializable]
    public abstract class AttackEvent : EntityEventArgs
    {
        /// <summary>
        /// Coordinates being attacked.
        /// </summary>
        public readonly NetCoordinates Coordinates;
        public readonly GameTick LastRealTick;

        protected AttackEvent(NetCoordinates coordinates, GameTick lastRealTick = default)
        {
            Coordinates = coordinates;
            LastRealTick = lastRealTick;
        }
    }

    /// <summary>
    ///     Event raised on entities that have been attacked.
    /// </summary>
    public sealed class AttackedEvent : EntityEventArgs
    {
        /// <summary>
        ///     Entity used to attack, for broadcast purposes.
        /// </summary>
        public EntityUid Used { get; }

        /// <summary>
        ///     Entity that triggered the attack.
        /// </summary>
        public EntityUid User { get; }

        /// <summary>
        ///     The original location that was clicked by the user.
        /// </summary>
        public EntityCoordinates ClickLocation { get; }

        public DamageSpecifier BonusDamage = new();

        public AttackedEvent(EntityUid used, EntityUid user, EntityCoordinates clickLocation)
        {
            Used = used;
            User = user;
            ClickLocation = clickLocation;
        }
    }
}
