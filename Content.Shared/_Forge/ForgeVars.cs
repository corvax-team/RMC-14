// hud.offer_mode_indicators_point_show -> Port From SS14 Corvax-Next

using Robust.Shared.Configuration;

namespace Content.Shared._Forge.ForgeVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class ForgeVars
{
    /// <summary>
    /// Offer item.
    /// </summary>
    public static readonly CVarDef<bool> OfferModeIndicatorsPointShow =
        CVarDef.Create("hud.offer_mode_indicators_point_show", true, CVar.ARCHIVE | CVar.CLIENTONLY);

    /// <summary>
    /// Responsible for turning on and off the bark system.
    /// </summary>
    public static readonly CVarDef<bool> BarksEnabled =
        CVarDef.Create("voice.barks_enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of Barks sound
    /// </summary>
    public static readonly CVarDef<float> BarksVolume =
        CVarDef.Create("voice.barks_volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Whether or not players can use mech guns outside of mechs.
    /// </summary>
    public static readonly CVarDef<bool> MechGunOutsideMech =
        CVarDef.Create("mech.gun_outside_mech", false, CVar.SERVER | CVar.REPLICATED);


    public static readonly CVarDef<string> DiscordApiUrl =
        CVarDef.Create("jerry.discord_api_url", "http://46.118.102.175:2424/api", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("jerry.discord_auth_enabled", true, CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<string> DiscordGuildID =
        CVarDef.Create("jerry.discord_guildId", "1136006498842063049", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<string> ApiKey =
        CVarDef.Create("jerry.discord_apikey", "f3a8c1e7b6d24f5a9e2c4d1b7f8a9c2e", CVar.CONFIDENTIAL | CVar.SERVERONLY);
}
