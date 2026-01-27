namespace Content.Shared._CCM.Pathogen.Protomorphs.HiveCocoon;

[ByRefEvent]
public record struct CocoonStartPupateAttemptEvent(EntityUid Cocoon, EntityUid User, EntityUid Target, bool Cancelled = false);
