using Robust.Shared.Configuration;

namespace Content.Shared._Forge;

/// <summary>
///     Forge module console variables (ported from Monolith).
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class ForgeVars
{
    /// <summary>
    ///     URL of the Discord auth remote service.
    /// </summary>
    public static readonly CVarDef<string> DiscordApiUrl =
        CVarDef.Create("jerry.discord_api_url", "", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    /// <summary>
    ///     Toggles the Discord auth gate before letting players into the server.
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("jerry.discord_auth_enabled", false, CVar.CONFIDENTIAL | CVar.SERVERONLY);

    /// <summary>
    ///     Discord guild ID used when resolving sponsor roles.
    /// </summary>
    public static readonly CVarDef<string> DiscordGuildID =
        CVarDef.Create("jerry.discord_guildId", "1222332535628103750", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    /// <summary>
    ///     Bearer key used to authenticate with the Discord auth API.
    /// </summary>
    public static readonly CVarDef<string> ApiKey =
        CVarDef.Create("jerry.discord_apikey", "", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    /// <summary>
    ///     Controls if the connections queue is enabled.
    ///     If enabled players will be added to a queue instead of being kicked after SoftMaxPlayers is reached.
    /// </summary>
    public static readonly CVarDef<bool> QueueEnabled =
        CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);

    /*
     * TTS (Text-To-Speech) — ported from Monolith.
     */

    /// <summary>
    ///     Master switch for the TTS feature (server side, replicated to clients).
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabled =
        CVarDef.Create("tts.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    ///     URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiUrl =
        CVarDef.Create("tts.api_url", "", CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.ARCHIVE);

    /// <summary>
    ///     Auth token of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiToken =
        CVarDef.Create("tts.api_token", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Amount of seconds before timeout for API.
    /// </summary>
    public static readonly CVarDef<int> TTSApiTimeout =
        CVarDef.Create("tts.api_timeout", 5, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Default volume setting of TTS sound.
    /// </summary>
    public static readonly CVarDef<float> TTSVolume =
        CVarDef.Create("tts.volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Whether the client wants local TTS playback enabled.
    /// </summary>
    public static readonly CVarDef<bool> LocalTTSEnabled =
        CVarDef.Create("tts.local_enabled", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    /// <summary>
    ///     Count of in-memory cached tts voice lines.
    /// </summary>
    public static readonly CVarDef<int> TTSMaxCache =
        CVarDef.Create("tts.max_cache", 250, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Tts rate limit values are accounted in periods of this size (seconds).
    ///     After the period has passed, the count resets.
    /// </summary>
    public static readonly CVarDef<float> TTSRateLimitPeriod =
        CVarDef.Create("tts.rate_limit_period", 2f, CVar.SERVERONLY);

    /// <summary>
    ///     How many tts preview messages are allowed in a single rate limit period.
    /// </summary>
    public static readonly CVarDef<int> TTSRateLimitCount =
        CVarDef.Create("tts.rate_limit_count", 3, CVar.SERVERONLY);
}
