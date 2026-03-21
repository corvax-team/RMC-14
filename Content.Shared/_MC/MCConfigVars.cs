using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

[CVarDefs]
public sealed class MCConfigVars : CVars
{
    public static readonly CVarDef<bool> ChatEmoji =
        CVarDef.Create("mc.chat.emoji", true, CVar.ARCHIVE | CVar.CLIENT);

    /**
     * Z-Levels
     */

    public static readonly CVarDef<int> ZLevelsPhysicsTickRate =
        CVarDef.Create("mc.z_levels.physics.tick_rate", 60, CVar.ARCHIVE);

    public static readonly CVarDef<bool> ZLevelsPhysicsClientSimulation =
        CVarDef.Create("mc.z_levels.physics.client_simulation", true, CVar.ARCHIVE | CVar.CLIENT);
}