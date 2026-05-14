// CM14 rework: non-RMC edit marker.
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using Content.Server._RMC14.Admin;
using Content.Server._CCM.Sponsorship;
using Content.Server._RMC14.Discord;
using Content.Server._RMC14.LinkAccount;
using Content.Server._RMC14.Mentor;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Server.Discord.DiscordLink;
using Content.Server.Players.RateLimiting;
using Content.Server.Preferences.Managers;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Chat;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Players.RateLimiting;
using Content.Shared.Localizations;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Replays;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Managers;

/// <summary>
///     Dispatches chat messages to clients.
/// </summary>
internal sealed partial class ChatManager : IChatManager
{
    private static readonly Dictionary<string, string> PatronOocColors = new()
    {
        // I had plans for multiple colors and those went nowhere so...
        { "nuclear_operative", "#aa00ff" },
        { "syndicate_agent", "#aa00ff" },
        { "revolutionary", "#aa00ff" }
    };

    [Dependency] private readonly IReplayRecordingManager _replay = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly PlayerRateLimitManager _rateLimitManager = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly DiscordChatLink _discordLink = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;

    // RMC14
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly CCMCustomizationManager _ccmCustomization = default!;
    [Dependency] private readonly CCMSponsorshipManager _ccmSponsorship = default!;
    [Dependency] private readonly RMCDiscordManager _discord = default!;
    [Dependency] private readonly MentorManager _mentor = default!;
    [Dependency] private readonly RMCChatBansManager _rmcChatBans = default!;

    /// <summary>
    /// The maximum length a player-sent message can be sent
    /// </summary>
    public int MaxMessageLength => _configurationManager.GetCVar(CCVars.ChatMaxMessageLength);

    private bool _oocEnabled = true;
    private bool _adminOocEnabled = true;

    private readonly Dictionary<NetUserId, ChatUser> _players = new();

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgChatMessage>();
        _netManager.RegisterNetMessage<MsgDeleteChatMessagesBy>();

        _configurationManager.OnValueChanged(CCVars.OocEnabled, OnOocEnabledChanged, true);
        _configurationManager.OnValueChanged(CCVars.AdminOocEnabled, OnAdminOocEnabledChanged, true);

        RegisterRateLimits();
    }

    private void OnOocEnabledChanged(bool val)
    {
        if (_oocEnabled == val) return;

        _oocEnabled = val;
        DispatchServerAnnouncementLoc(val ? "chat-manager-ooc-chat-enabled-message" : "chat-manager-ooc-chat-disabled-message");
    }

    private void OnAdminOocEnabledChanged(bool val)
    {
        if (_adminOocEnabled == val) return;

        _adminOocEnabled = val;
        DispatchServerAnnouncementLoc(val ? "chat-manager-admin-ooc-chat-enabled-message" : "chat-manager-admin-ooc-chat-disabled-message");
    }

        public void DeleteMessagesBy(NetUserId uid)
        {
            if (!_players.TryGetValue(uid, out var user))
                return;

        var msg = new MsgDeleteChatMessagesBy { Key = user.Key, Entities = user.Entities };
        _netManager.ServerSendToAll(msg);
    }

    [return: NotNullIfNotNull(nameof(author))]
    public ChatUser? EnsurePlayer(NetUserId? author)
    {
        if (author == null)
            return null;

        ref var user = ref CollectionsMarshal.GetValueRefOrAddDefault(_players, author.Value, out var exists);
        if (!exists || user == null)
            user = new ChatUser(_players.Count);

        return user;
    }

    #region Server Announcements

    public void DispatchServerAnnouncement(string message, Color? colorOverride = null)
    {
        string WrapForCurrentCulture() => Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message)));
        var wrappedMessage = WrapForCurrentCulture();

        foreach (var session in _player.Sessions)
        {
            ChatMessageToOne(
                ChatChannel.Server,
                message,
                WithChannelCulture(session.Channel, WrapForCurrentCulture),
                EntityUid.Invalid,
                hideChat: false,
                session.Channel,
                colorOverride);
        }

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.Server, message, wrappedMessage, NetEntity.Invalid, null, false, colorOverride));
        Logger.InfoS("SERVER", message);

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server announcement: {message}");
    }

    public void DispatchServerAnnouncementLoc(string messageLocId, (string, object)[]? args = null, Color? colorOverride = null)
    {
        args ??= Array.Empty<(string, object)>();

        var replayMessage = Loc.GetString(messageLocId, args);
        string WrapForCurrentCulture(string message) => Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message)));
        var replayWrappedMessage = WrapForCurrentCulture(replayMessage);

        foreach (var session in _player.Sessions)
        {
            var localizedMessage = WithChannelCulture(session.Channel, () => Loc.GetString(messageLocId, args));
            var localizedWrapped = WithChannelCulture(session.Channel, () => WrapForCurrentCulture(localizedMessage));
            ChatMessageToOne(
                ChatChannel.Server,
                localizedMessage,
                localizedWrapped,
                EntityUid.Invalid,
                hideChat: false,
                session.Channel,
                colorOverride);
        }

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.Server, replayMessage, replayWrappedMessage, NetEntity.Invalid, null, false, colorOverride));
        Logger.InfoS("SERVER", replayMessage);
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server announcement loc: {messageLocId}");
    }

    public void DispatchServerMessage(ICommonSession player, string message, bool suppressLog = false)
    {
        var wrappedMessage = WithChannelCulture(player.Channel, () => Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message))));
        ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, player.Channel);

        if (!suppressLog)
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server message to {player:Player}: {message}");
    }

    public void DispatchServerMessageLoc(ICommonSession player, string messageLocId, (string, object)[]? args = null, bool suppressLog = false)
    {
        args ??= Array.Empty<(string, object)>();
        var message = WithChannelCulture(player.Channel, () => Loc.GetString(messageLocId, args));
        var wrappedMessage = WithChannelCulture(player.Channel, () => Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message))));
        ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, player.Channel);

        if (!suppressLog)
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server message loc to {player:Player}: {messageLocId}");
    }

    public void SendAdminAnnouncement(string message, AdminFlags? flagBlacklist, AdminFlags? flagWhitelist)
    {
        var clients = _adminManager.ActiveAdmins.Where(p =>
        {
            var adminData = _adminManager.GetAdminData(p);

            DebugTools.AssertNotNull(adminData);

            if (adminData == null)
                return false;

            if (flagBlacklist != null && adminData.HasFlag(flagBlacklist.Value))
                return false;

            return flagWhitelist == null || adminData.HasFlag(flagWhitelist.Value);

        }).Select(p => p.Channel);

        string WrapForCurrentCulture() => Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
            ("message", FormattedMessage.EscapeText(message)));
        var wrappedMessage = WrapForCurrentCulture();

        foreach (var client in clients)
        {
            ChatMessageToOne(
                ChatChannel.Admin,
                message,
                WithChannelCulture(client, WrapForCurrentCulture),
                default,
                false,
                client);
        }

        if (_configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            _replay.RecordServerMessage(new ChatMessage(ChatChannel.Admin, message, wrappedMessage, NetEntity.Invalid, null, false));
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Admin announcement: {message}");
    }

    public void SendAdminAnnouncementLoc(string messageLocId, (string, object)[]? args = null, AdminFlags? flagBlacklist = null, AdminFlags? flagWhitelist = null)
    {
        args ??= Array.Empty<(string, object)>();
        var clients = _adminManager.ActiveAdmins.Where(p =>
        {
            var adminData = _adminManager.GetAdminData(p);

            DebugTools.AssertNotNull(adminData);

            if (adminData == null)
                return false;

            if (flagBlacklist != null && adminData.HasFlag(flagBlacklist.Value))
                return false;

            return flagWhitelist == null || adminData.HasFlag(flagWhitelist.Value);

        }).Select(p => p.Channel);

        var replayMessage = Loc.GetString(messageLocId, args);
        string WrapForCurrentCulture(string message) => Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
            ("message", FormattedMessage.EscapeText(message)));
        var replayWrappedMessage = WrapForCurrentCulture(replayMessage);

        foreach (var client in clients)
        {
            var localizedMessage = WithChannelCulture(client, () => Loc.GetString(messageLocId, args));
            var localizedWrappedMessage = WithChannelCulture(client, () => WrapForCurrentCulture(localizedMessage));
            ChatMessageToOne(
                ChatChannel.Admin,
                localizedMessage,
                localizedWrappedMessage,
                default,
                false,
                client);
        }

        if (_configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            _replay.RecordServerMessage(new ChatMessage(ChatChannel.Admin, replayMessage, replayWrappedMessage, NetEntity.Invalid, null, false));
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Admin announcement loc: {messageLocId}");
    }

    public void SendAdminAnnouncementMessage(ICommonSession player, string message, bool suppressLog = true)
    {
        var wrappedMessage = WithChannelCulture(player.Channel, () => Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
            ("message", FormattedMessage.EscapeText(message))));
        ChatMessageToOne(ChatChannel.Admin, message, wrappedMessage, default, false, player.Channel);
    }

    public void SendAdminAlert(string message)
    {
        var clients = _adminManager.ActiveAdmins
            .Where(p => _adminManager.HasAdminFlag(p, AdminFlags.EditNotes)) // RMC14
            .Select(p => p.Channel);

        string WrapForCurrentCulture() => Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
            ("message", FormattedMessage.EscapeText(message)));
        var wrappedMessage = WrapForCurrentCulture();

        foreach (var client in clients)
        {
            ChatMessageToOne(
                ChatChannel.AdminAlert,
                message,
                WithChannelCulture(client, WrapForCurrentCulture),
                default,
                false,
                client);
        }

        if (_configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            _replay.RecordServerMessage(new ChatMessage(ChatChannel.AdminAlert, message, wrappedMessage, NetEntity.Invalid, null, false));
    }

    public void SendAdminAlert(EntityUid player, string message)
    {
        var mindSystem = _entityManager.System<SharedMindSystem>();
        if (!mindSystem.TryGetMind(player, out var mindId, out var mind))
        {
            SendAdminAlert(message);
            return;
        }

        var adminSystem = _entityManager.System<AdminSystem>();
        var antag = mind.UserId != null && (adminSystem.GetCachedPlayerInfo(mind.UserId.Value)?.Antag ?? false);

        // We shouldn't be repeating this but I don't want to touch any more chat code than necessary
        var playerName = mind.UserId is { } userId && _player.TryGetSessionById(userId, out var session)
            ? session.Name
            : "Unknown";

        SendAdminAlert($"{playerName}{(antag ? " (ANTAG)" : "")} {message}");
    }

    public void SendHookOOC(string sender, string message)
    {
        if (!_oocEnabled && _configurationManager.GetCVar(CCVars.DisablingOOCDisablesRelay))
        {
            return;
        }
        string WrapForCurrentCulture() => Loc.GetString("chat-manager-send-hook-ooc-wrap-message",
            ("senderName", sender),
            ("message", FormattedMessage.EscapeText(message)));
        var wrappedMessage = WrapForCurrentCulture();
        foreach (var session in _player.Sessions)
        {
            ChatMessageToOne(
                ChatChannel.OOC,
                message,
                WithChannelCulture(session.Channel, WrapForCurrentCulture),
                source: EntityUid.Invalid,
                hideChat: false,
                session.Channel);
        }

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.OOC, message, wrappedMessage, NetEntity.Invalid, null, false));
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Hook OOC from {sender}: {message}");
    }

    public void SendHookAdmin(string sender, string message)
    {
        var clients = _adminManager.ActiveAdmins.Select(p => p.Channel);

        foreach (var client in clients)
        {
            var wrappedMessage = WithChannelCulture(client, () => Loc.GetString("chat-manager-send-hook-admin-wrap-message",
                ("senderName", sender),
                ("message", FormattedMessage.EscapeText(message))));
            ChatMessageToOne(
                ChatChannel.AdminChat,
                message,
                wrappedMessage,
                source: EntityUid.Invalid,
                hideChat: false,
                client: client,
                recordReplay: false,
                audioPath: _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundPath),
                audioVolume: _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundVolume));
        }

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Hook admin from {sender}: {message}");
    }

    #endregion

    #region Public OOC Chat API

    /// <summary>
    ///     Called for a player to attempt sending an OOC, out-of-game. message.
    /// </summary>
    /// <param name="player">The player sending the message.</param>
    /// <param name="message">The message.</param>
    /// <param name="type">The type of message.</param>
    public void TrySendOOCMessage(ICommonSession player, string message, OOCChatType type)
    {
        if (HandleRateLimit(player) != RateLimitStatus.Allowed)
            return;

        // Check if message exceeds the character limit
        if (message.Length > MaxMessageLength)
        {
            DispatchServerMessageLoc(player, "chat-manager-max-message-length-exceeded-message",
                new[] { ("limit", (object) MaxMessageLength) });
            return;
        }

        switch (type)
        {
            case OOCChatType.OOC:
                SendOOC(player, message);
                break;
            case OOCChatType.Admin:
                SendAdminChat(player, message);
                break;
            case OOCChatType.Mentor:
                SendMentorChat(player, message);
                break;
        }
    }

    #endregion

    #region Private API

    private void SendOOC(ICommonSession player, string message)
    {
        if (_adminManager.IsAdmin(player))
        {
            if (!_adminOocEnabled)
            {
                return;
            }
        }
        else if (!_oocEnabled)
        {
            return;
        }

        // RMC14
        if (_rmcChatBans.IsChatBanned(player.UserId, ChatType.Ooc))
        {
            var bannedMsg = Loc.GetString("rmc-chat-bans-banned");
            ChatMessageToOne(ChatChannel.Server, bannedMsg, bannedMsg, default, false, player.Channel);
            return;
        }

        var formattedMessage = FormattedMessage.EscapeText(message);

        Color? colorOverride = null;
        var displayName = BuildOocDisplayName(player);

        string WrapOocForCurrentCulture() => Loc.GetString("chat-manager-send-ooc-wrap-message",
            ("playerName", FormattedMessage.EscapeText(displayName)),
            ("message", formattedMessage));

        string WrapPatronForCurrentCulture(string color) => Loc.GetString("chat-manager-send-ooc-patron-wrap-message",
            ("patronColor", color),
            ("playerName", FormattedMessage.EscapeText(displayName)),
            ("message", formattedMessage));

        var replayWrappedMessage = WrapOocForCurrentCulture();
        if (_adminManager.HasAdminFlag(player, AdminFlags.NameColor))
        {
            var prefs = _preferencesManager.GetPreferences(player.UserId);
            colorOverride = prefs.AdminOOCColor;
        }
        foreach (var session in _player.Sessions)
        {
            var wrappedMessage = WithChannelCulture(session.Channel, () =>
            {
                if (_ccmCustomization.TryGetChatColorHex(player.UserId, false, out var customColor))
                    return WrapPatronForCurrentCulture(customColor);

                if (_ccmSponsorship.TryGetChatColorHex(player.UserId, false, out var sponsorColor))
                    return WrapPatronForCurrentCulture(sponsorColor);

                return WrapOocForCurrentCulture();
            });

            ChatMessageToOne(
                ChatChannel.OOC,
                message,
                wrappedMessage,
                EntityUid.Invalid,
                hideChat: false,
                session.Channel,
                colorOverride: colorOverride,
                author: player.UserId);
                        }

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.OOC, message, replayWrappedMessage, NetEntity.Invalid, EnsurePlayer(player.UserId)?.Key, false, colorOverride, speechStyleClass: null, repeatCheckSender: true));
        _discordLink.SendMessage(message, player.Name, ChatChannel.OOC);
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"OOC from {player:Player}: {message}");
    }

    private string BuildOocDisplayName(ICommonSession player)
    {
        if (!_ccmCustomization.TryGetOocTagText(player.UserId, out var tagText))
            return player.Name;

        return $"[{tagText}] {player.Name}";
    }

    private void SendAdminChat(ICommonSession player, string message)
    {
        if (!_adminManager.IsAdmin(player))
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Extreme, $"{player:Player} attempted to send admin message but was not admin");
            return;
        }

        var clients = _adminManager.ActiveAdmins.Select(p => p.Channel);
        string WrapForCurrentCulture() => Loc.GetString("chat-manager-send-admin-chat-wrap-message",
                                        ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
                                        ("playerName", player.Name), ("message", FormattedMessage.EscapeText(message)));

        _discord.SendDiscordAdminMessage(player.Name, message);

        foreach (var client in clients)
        {
            var isSource = client != player.Channel;
            ChatMessageToOne(ChatChannel.AdminChat,
                message,
                WithChannelCulture(client, WrapForCurrentCulture),
                default,
                false,
                client,
                audioPath: isSource ? _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundPath) : default,
                audioVolume: isSource ? _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundVolume) : default,
                author: player.UserId);
        }

        _discordLink.SendMessage(message, player.Name, ChatChannel.AdminChat);
        _adminLogger.Add(LogType.Chat, $"Admin chat from {player:Player}: {message}");
    }

    private void SendMentorChat(ICommonSession player, string message)
    {
        if (!_mentor.IsMentor(player.UserId))
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Extreme, $"{player:Player} attempted to send mentor chat message but was not mentor");
            return;
        }

        var clients = _mentor.GetActiveMentors().Select(p => p.Channel);
        string WrapForCurrentCulture() => Loc.GetString("chat-manager-send-admin-chat-wrap-message",
                                        ("adminChannelName", "MENTOR"),
                                        ("playerName", player.Name), ("message", FormattedMessage.EscapeText(message)));

        _discord.SendDiscordMentorMessage(player.Name, message);

        foreach (var client in clients)
        {
            var isSource = client != player.Channel;
            ChatMessageToOne(ChatChannel.MentorChat,
                message,
                WithChannelCulture(client, WrapForCurrentCulture),
                default,
                false,
                client,
                audioPath: isSource ? _netConfigManager.GetClientCVar(client, RMCCVars.RMCMentorChatSound) : default,
                audioVolume: isSource ? _netConfigManager.GetClientCVar(client, RMCCVars.RMCMentorChatVolume) : default,
                author: player.UserId);
        }

        _adminLogger.Add(LogType.Chat, $"Mentor chat from {player:Player}: {message}");
    }

    #endregion

    #region Utility

    private string GetClientLocaleCode(INetChannel channel)
    {
        var locale = _netConfigManager.GetClientCVar(channel, CCVars.ClientLocale);
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

    public void ChatMessageToOne(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, INetChannel client, Color? colorOverride = null, bool recordReplay = false, string? audioPath = null, float audioVolume = 0, NetUserId? author = null, bool hidePopup = false)
    {
        var user = author == null ? null : EnsurePlayer(author);
        var netSource = _entityManager.GetNetEntity(source);
        user?.AddEntity(netSource);

        var msg = new ChatMessage(channel, message, wrappedMessage, netSource, user?.Key, hideChat, colorOverride, audioPath, audioVolume, hidePopup, speechStyleClass: _entityManager.GetComponentOrNull<RMCSpeechBubbleSpecificStyleComponent>(source)?.SpeechStyleClass, repeatCheckSender: !_entityManager.HasComponent<ChatRepeatIgnoreSenderComponent>(source));
        ChatMessageToOne(msg, client, recordReplay);
    }

    public void ChatMessageToOne(ChatMessage message, INetChannel client, bool recordReplay = false)
    {
        DispatchChatMessageToClient(client, message);
        RecordReplayIfNeeded(message, recordReplay);
    }

    public void ChatMessageToMany(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, IEnumerable<INetChannel> clients, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0, NetUserId? author = null)
        => ChatMessageToMany(channel, message, wrappedMessage, source, hideChat, recordReplay, clients.ToList(), colorOverride, audioPath, audioVolume, author);

    public void ChatMessageToMany(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, List<INetChannel> clients, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0, NetUserId? author = null)
    {
        var user = author == null ? null : EnsurePlayer(author);
        var netSource = _entityManager.GetNetEntity(source);
        user?.AddEntity(netSource);

        var msg = new ChatMessage(channel, message, wrappedMessage, netSource, user?.Key, hideChat, colorOverride, audioPath, audioVolume, speechStyleClass: _entityManager.GetComponentOrNull<RMCSpeechBubbleSpecificStyleComponent>(source)?.SpeechStyleClass, repeatCheckSender: !_entityManager.HasComponent<ChatRepeatIgnoreSenderComponent>(source));
        foreach (var client in clients)
        {
            DispatchChatMessageToClient(client, msg);
        }

        RecordReplayIfNeeded(msg, recordReplay);
    }

    public void ChatMessageToManyFiltered(Filter filter, ChatChannel channel, string message, string wrappedMessage, EntityUid source,
        bool hideChat, bool recordReplay, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0)
    {
        if (!recordReplay && !filter.Recipients.Any())
            return;

        var clients = new List<INetChannel>();
        foreach (var recipient in filter.Recipients)
        {
            clients.Add(recipient.Channel);
        }

        ChatMessageToMany(channel, message, wrappedMessage, source, hideChat, recordReplay, clients, colorOverride, audioPath, audioVolume);
    }

    public void ChatMessageToAll(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0, NetUserId? author = null)
    {
        var user = author == null ? null : EnsurePlayer(author);
        var netSource = _entityManager.GetNetEntity(source);
        user?.AddEntity(netSource);

        var msg = new ChatMessage(channel, message, wrappedMessage, netSource, user?.Key, hideChat, colorOverride, audioPath, audioVolume, speechStyleClass: _entityManager.GetComponentOrNull<RMCSpeechBubbleSpecificStyleComponent>(source)?.SpeechStyleClass, repeatCheckSender: !_entityManager.HasComponent<ChatRepeatIgnoreSenderComponent>(source));
        foreach (var session in _player.Sessions)
        {
            DispatchChatMessageToClient(session.Channel, msg);
        }

        RecordReplayIfNeeded(msg, recordReplay);
    }

    public bool MessageCharacterLimit(ICommonSession? player, string message)
    {
        var isOverLength = false;

        // Non-players don't need to be checked.
        if (player == null)
            return false;

        // Check if message exceeds the character limit if the sender is a player
        if (message.Length > MaxMessageLength)
        {
            DispatchServerMessageLoc(player, "chat-manager-max-message-length-exceeded-message",
                new[] { ("limit", (object) MaxMessageLength) });

            isOverLength = true;
        }

        return isOverLength;
    }

    #endregion
}

public enum OOCChatType : byte
{
    OOC,
    Admin,
    Mentor,
}
