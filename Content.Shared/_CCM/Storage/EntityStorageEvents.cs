using Robust.Shared.Containers;

namespace Content.Shared._CCM.Storage;

[ByRefEvent]
public record struct EntityStorageIntoContainerAttemptEvent(BaseContainer Container, bool Cancelled = false);
