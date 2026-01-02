/*
Copyright (c) 2025 Inconnu (Discord: Inconnu1337).
All Rights Reserved.

An exclusive license is granted to Denlero (Discord: Denlero)
for the Corvax Colonial Marines project, with full rights
to use, modify, distribute, and sublicense.
Third-party use requires Denlero's consent.
*/
using Content.Shared._CCM.Vehicle.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedVehicleSystem), typeof(SharedMoverController))]
public sealed partial class VehicleMovementComponent : Component
{
    [DataField, AutoNetworkedField]
    public int CurrentMomentum;

    [DataField, AutoNetworkedField]
    public int MaxMomentum = 2;

    [DataField, AutoNetworkedField]
    public int MinimumStepsForMomentum = 2;

    [DataField, AutoNetworkedField]
    public float StepIncrement = 1.0f;

    [DataField, AutoNetworkedField]
    public float DistanceMoved;

    [DataField, AutoNetworkedField]
    public int Steps;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastMovementTime;

    [DataField, AutoNetworkedField]
    public TimeSpan MomentumDecayDelay = TimeSpan.FromSeconds(0.5);

    [DataField]
    public float MomentumTurnLossFactor = 0.5f;

    [DataField]
    public float MaxMomentumSpeedBonus = 0.75f;

    [DataField]
    public float SpeedPerMomentum = 0.15f;

    [DataField, AutoNetworkedField]
    public bool Blocked;

    [DataField, AutoNetworkedField]
    public Direction LastMoveDirection;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastInputTime;

    [DataField, AutoNetworkedField]
    public TimeSpan InputDelay = TimeSpan.FromSeconds(0.2);

    [DataField, AutoNetworkedField]
    public SoundSpecifier? MovementSound = new SoundPathSpecifier("/Audio/_CCM14/tank_driving.ogg", AudioParams.Default.WithVolume(-4));

    [DataField, AutoNetworkedField]
    public EntityUid? AudioStream;
}
