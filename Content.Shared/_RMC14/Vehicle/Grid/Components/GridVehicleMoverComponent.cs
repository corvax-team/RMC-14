using System;
using System.Numerics;
using Content.Shared._RMC14.Stun;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Shared.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(Content.Shared.Vehicle.GridVehicleMoverSystem), Other = AccessPermissions.ReadWrite)]
public sealed partial class GridVehicleMoverComponent : Component
{
    [AutoNetworkedField]
    public Vector2i CurrentTile;

    [AutoNetworkedField]
    public Vector2i TargetTile;

    [AutoNetworkedField]
    public Vector2 Position;

    [AutoNetworkedField]
    public Vector2i CurrentDirection;

    [AutoNetworkedField]
    public float CurrentSpeed;

    [DataField, AutoNetworkedField]
    public float MaxSpeed = 11f;

    [DataField, AutoNetworkedField]
    public float Acceleration = 7f;

    [DataField, AutoNetworkedField]
    public float Deceleration = 12f;

    [DataField, AutoNetworkedField]
    public float MaxReverseSpeed = 4f;

    [DataField, AutoNetworkedField]
    public float ReverseAcceleration = 4f;

    [DataField, AutoNetworkedField]
    public float FrontOffset = 0f;

    [DataField, AutoNetworkedField]
    public float PushCooldown = 0f;

    [DataField, AutoNetworkedField]
    public float TurnDelay = 0.08f;

    [DataField, AutoNetworkedField]
    public bool TurnInPlace = false;

    [DataField, AutoNetworkedField]
    public float TurnInPlaceMaxSpeed = 0.35f;

    [AutoNetworkedField]
    public TimeSpan NextPushTime;

    [AutoNetworkedField]
    public TimeSpan NextTurnTime;

    [AutoNetworkedField]
    public TimeSpan InPlaceTurnBlockUntil;

    [AutoNetworkedField]
    public bool IsCommittedToMove;

    [AutoNetworkedField]
    public bool IsPushMove;

    [AutoNetworkedField]
    public bool IsMoving;

    [DataField, AutoNetworkedField]
    public RMCSizes? XenoBlockMinimumSize;

    [DataField, AutoNetworkedField]
    public bool CanXenosPush = true;

    [DataField, AutoNetworkedField]
    public RMCSizes? XenoPushMinimumSize;

    [NonSerialized]
    public EntityUid? SyncedGrid;

    [AutoNetworkedField]
    public float SmashSlowdownMultiplier = 1f;

    [AutoNetworkedField]
    public TimeSpan SmashSlowdownUntil;
    // CCM14-start
    [DataField, AutoNetworkedField]
    public double MobCollisionDamage = 8;

    [DataField, AutoNetworkedField]
    public double UnpoweredDoorCollisionDamage = 1000;

    [DataField, AutoNetworkedField]
    public TimeSpan MobCollisionKnockdown = TimeSpan.FromSeconds(1.5);

    [DataField, AutoNetworkedField]
    public TimeSpan MobCollisionCooldown = TimeSpan.FromSeconds(0.75);

    [DataField, AutoNetworkedField]
    public ProtoId<DamageTypePrototype> CollisionDamageType = "Blunt";
    // CCM14-end
}
