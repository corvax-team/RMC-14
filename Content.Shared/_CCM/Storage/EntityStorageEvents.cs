using System.Numerics;
using Content.Shared.Atmos;
using Content.Shared.Physics;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Storage.Components;

[ByRefEvent]
public record struct EntityStorageIntoContainerAttemptEvent(BaseContainer Container, bool Cancelled = false);
