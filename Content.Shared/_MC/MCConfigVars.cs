using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

[CVarDefs]
public sealed class MCConfigVars : CVars
{
    public static readonly CVarDef<bool> ChatEmoji =
        CVarDef.Create("mc.chat.emoji", false, CVar.ARCHIVE | CVar.CLIENT);


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

    public static readonly CVarDef<bool> ZLevelsEnabled =
        CVarDef.Create("mc.z_levels.enabled", false, CVar.ARCHIVE | CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> ZLevelsPhysicsTickRate =
        CVarDef.Create("mc.z_levels.physics.tick_rate", 20, CVar.ARCHIVE);

    public static readonly CVarDef<bool> ZLevelsPhysicsClientSimulation =
        CVarDef.Create("mc.z_levels.physics.client_simulation", false, CVar.ARCHIVE | CVar.CLIENT);

    public static readonly CVarDef<int> ZLevelsViewerMaxPreloadBelowDepth =
        CVarDef.Create("mc.z_levels.viewer.max_preload_below_depth", 1, CVar.ARCHIVE | CVar.SERVERONLY);

    public static readonly CVarDef<bool> ZLevelsViewerKeepAboveHot =
        CVarDef.Create("mc.z_levels.viewer.keep_above_hot", false, CVar.ARCHIVE | CVar.SERVERONLY);

    public static readonly CVarDef<int> ZLevelsRenderMaxBelowDepth =
        CVarDef.Create("mc.z_levels.render.max_below_depth", 1, CVar.ARCHIVE | CVar.CLIENTONLY);

    public static readonly CVarDef<string> ZLevelsRenderLowerFx =
        CVarDef.Create("mc.z_levels.render.lower_fx", "blur", CVar.ARCHIVE | CVar.CLIENTONLY);
}
