// CM14 rework: non-RMC edit marker.
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using System;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Localizations;
using Robust.Shared.Console;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Motd;

/// <summary>
/// The system that handles broadcasting the Message Of The Day to players when they join the lobby/the MOTD changes/they ask for it to be printed.
/// </summary>
public sealed class MOTDSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;

    /// <summary>
    /// The cached value of the Message of the Day. Used for fast access.
    /// </summary>
    private string _messageOfTheDay = "";

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configurationManager, CCVars.MOTD, OnMOTDChanged, invokeImmediately: true);
        SubscribeLocalEvent<PlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);
    }

    /// <summary>
    /// Sends the Message Of The Day, if any, to all connected players.
    /// </summary>
    public void TrySendMOTD()
    {
        if (string.IsNullOrEmpty(_messageOfTheDay))
            return;

        foreach (var session in _playerManager.Sessions)
        {
            TrySendMOTD(session);
        }
    }

    /// <summary>
    /// Sends the Message Of The Day, if any, to a specific player.
    /// </summary>
    public void TrySendMOTD(ICommonSession player)
    {
        if (string.IsNullOrEmpty(_messageOfTheDay))
            return;

        var wrappedMessage = WithChannelCulture(player.Channel, () => Loc.GetString("motd-wrap-message", ("motd", _messageOfTheDay)));
        _chatManager.ChatMessageToOne(ChatChannel.Server, _messageOfTheDay, wrappedMessage, source: EntityUid.Invalid, hideChat: false, client: player.Channel);
    }

    /// <summary>
    /// Sends the Message Of The Day, if any, to a specific player's console and chat.
    /// </summary>
    /// <remarks>
    /// This is used by the MOTD console command because we can't tell whether the player is using `console or /console so we send the message to both.
    /// </remarks>
    public void TrySendMOTD(IConsoleShell shell)
    {
        if (string.IsNullOrEmpty(_messageOfTheDay))
            return;

        var wrappedMessage = Loc.GetString("motd-wrap-message", ("motd", _messageOfTheDay));
        shell.WriteLine(wrappedMessage);
        if (shell.Player is { } player)
            _chatManager.ChatMessageToOne(
                ChatChannel.Server,
                _messageOfTheDay,
                WithChannelCulture(player.Channel, () => Loc.GetString("motd-wrap-message", ("motd", _messageOfTheDay))),
                source: EntityUid.Invalid,
                hideChat: false,
                client: player.Channel);
    }

    private string GetClientLocaleCode(INetChannel channel)
    {
        var locale = _netConfig.GetClientCVar(channel, CCVars.ClientLocale);
        return string.IsNullOrWhiteSpace(locale) ? "ru-RU" : locale;
    }

    private T WithChannelCulture<T>(INetChannel channel, Func<T> action)
    {
        var locale = GetClientLocaleCode(channel);
        var oldCulture = _contentLoc.CurrentCultureCode;
        if (oldCulture.Equals(locale, StringComparison.OrdinalIgnoreCase))
            return action();

        _contentLoc.SetCulture(locale);
        try
        {
            return action();
        }
        finally
        {
            _contentLoc.SetCulture(oldCulture);
        }
    }

    #region Event Handlers

    /// <summary>
    /// Posts the Message Of The Day to any players who join the lobby.
    /// </summary>
    private void OnPlayerJoinedLobby(PlayerJoinedLobbyEvent ev)
    {
        TrySendMOTD(ev.PlayerSession);
    }

    /// <summary>
    /// Broadcasts changes to the Message Of The Day to all players.
    /// </summary>
    private void OnMOTDChanged(string val)
    {
        if (val == _messageOfTheDay)
            return;

        _messageOfTheDay = val;
        TrySendMOTD();
    }

    #endregion Event Handlers
}
