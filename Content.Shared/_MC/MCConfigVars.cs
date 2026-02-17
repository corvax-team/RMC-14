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

    // /**
    //  * Round
    //  */

    // public static readonly CVarDef<int> RoundForceEndHijackTimeMinutes =
    //     CVarDef.Create("mc.round.hijack_end_time_minutes", 25, CVar.REPLICATED | CVar.SERVER);

    // public static readonly CVarDef<bool> RoundCanEnd =
    //     CVarDef.Create("mc.round.can_end", true, CVar.REPLICATED | CVar.SERVER);

    /**
     * Z-Levels
     */

    public static readonly CVarDef<int> ZLevelsPhysicsTickRate =
        CVarDef.Create("mc.z_levels.physics.tick_rate", 60, CVar.ARCHIVE);

    public static readonly CVarDef<bool> ZLevelsPhysicsClientSimulation =
        CVarDef.Create("mc.z_levels.physics.client_simulation", true, CVar.ARCHIVE | CVar.CLIENT);
}