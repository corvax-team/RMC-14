using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

public sealed partial class RMCCVars
{
    public static readonly CVarDef<bool> RMCChatTranslateEnabled =
        CVarDef.Create("rmc.chat_translate_enabled", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> RMCChatTranslateApi =
        CVarDef.Create("rmc.chat_translate_api", "http://127.0.0.1:5000/translate", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> RMCChatTranslateSource =
        CVarDef.Create("rmc.chat_translate_source", "auto", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> RMCChatTranslateTarget =
        CVarDef.Create("rmc.chat_translate_target", "ru", CVar.CLIENTONLY | CVar.ARCHIVE);
}
