using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Content.Server.Database;
using Content.Shared.CCVar;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.LinkAccount;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountManager : IPostInjectInit
{
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private ISawmill _sawmill = default!;
    private readonly Dictionary<NetUserId, TimeSpan> _lastRequest = new();
    private readonly TimeSpan _minimumWait = TimeSpan.FromSeconds(0.5);
    private readonly Dictionary<NetUserId, SharedRMCPatronFull> _connected = new();
    private readonly Dictionary<NetUserId, SharedRMCPatron> _allPatrons = [];
    private readonly HashSet<Guid> _figurines = [];
    private static readonly JsonSerializerOptions StateJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public event Action? PatronsReloaded;
    public event Action<(NetUserId Id, SharedRMCPatronFull Patron)>? PatronUpdated;

    private async Task LoadData(ICommonSession player, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await RefreshConnected(player.UserId, cancel);
    }

    private void FinishLoad(ICommonSession player)
    {
        SendPatronStatus(player);
    }

    private void ClientDisconnected(ICommonSession player)
    {
        _connected.Remove(player.UserId);
    }

    private void SendPatronStatus(ICommonSession player)
    {
        var connected = _connected.GetValueOrDefault(player.UserId);
        _net.ServerSendMessage(new LinkAccountStatusMsg { Patron = connected }, player.Channel);
        SendPatrons(player);
    }

    private void SendPatronStatus(NetUserId user)
    {
        if (_player.TryGetSessionById(user, out var session))
            SendPatronStatus(session);
    }

    private void OnRequest(LinkAccountRequestMsg message)
    {
        _net.ServerSendMessage(new LinkAccountCodeMsg { Code = Guid.Empty }, message.MsgChannel);
    }

    private void OnDiscordOAuthLinkRequest(RMCDiscordOAuthLinkRequestMsg message)
    {
        var time = _timing.RealTime;
        var userId = message.MsgChannel.UserId;
        if (_lastRequest.TryGetValue(userId, out var last) &&
            last + _minimumWait > time)
        {
            return;
        }

        _lastRequest[userId] = time;

        try
        {
            var url = BuildDiscordOAuthLink(message);
            _net.ServerSendMessage(new RMCDiscordOAuthLinkMsg { Url = url }, message.MsgChannel);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to generate Discord OAuth link:\n{e}");
            _net.ServerSendMessage(new RMCDiscordOAuthLinkMsg { Error = "oauth-link-failed" }, message.MsgChannel);
        }
    }

    private async void OnClearGhostColor(RMCClearGhostColorMsg message)
    {
        await RunLogged(async () =>
        {
            await _db.SetGhostColor(message.MsgChannel.UserId.UserId, null);
            await RefreshAndSend(message.MsgChannel.UserId);
        });
    }

    private async void OnChangeGhostColor(RMCChangeGhostColorMsg message)
    {
        await RunLogged(async () =>
        {
            await _db.SetGhostColor(message.MsgChannel.UserId.UserId, System.Drawing.Color.FromArgb(message.Color.ToArgb()));
            await RefreshAndSend(message.MsgChannel.UserId);
        });
    }

    private async void OnChangeLobbyMessage(RMCChangeLobbyMessageMsg message)
    {
        await RunLogged(async () =>
        {
            var text = message.Text ?? string.Empty;
            if (text.Length > SharedRMCLobbyMessage.CharacterLimit)
                text = text[..SharedRMCLobbyMessage.CharacterLimit];

            await _db.SetLobbyMessage(message.MsgChannel.UserId.UserId, text);
            await RefreshAndSend(message.MsgChannel.UserId);
        });
    }

    private async void OnChangeMarineShoutout(RMCChangeMarineShoutoutMsg message)
    {
        await RunLogged(async () =>
        {
            var name = message.Name ?? string.Empty;
            if (name.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
                name = name[..SharedRMCRoundEndShoutouts.CharacterLimit];

            await _db.SetMarineShoutout(message.MsgChannel.UserId.UserId, name);
            await RefreshAndSend(message.MsgChannel.UserId);
        });
    }

    private async void OnChangeXenoShoutout(RMCChangeXenoShoutoutMsg message)
    {
        await RunLogged(async () =>
        {
            var name = message.Name ?? string.Empty;
            if (name.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
                name = name[..SharedRMCRoundEndShoutouts.CharacterLimit];

            await _db.SetXenoShoutout(message.MsgChannel.UserId.UserId, name);
            await RefreshAndSend(message.MsgChannel.UserId);
        });
    }

    public async Task RefreshAllPatrons()
    {
        _allPatrons.Clear();
        _figurines.Clear();
        var patrons = await _db.GetAllPatrons();
        foreach (var patron in patrons)
        {
            _allPatrons[new NetUserId(patron.PlayerId)] = ToSharedPatron(patron);
            if (patron.Tier.Figurines)
                _figurines.Add(patron.PlayerId);
        }

        PatronsReloaded?.Invoke();
    }

    public void SendPatronsToAll()
    {
        _net.ServerSendToAll(new RMCPatronListMsg { Patrons = [.. _allPatrons.Values] });
    }

    private void SendPatrons(ICommonSession player)
    {
        _net.ServerSendMessage(new RMCPatronListMsg { Patrons = [.. _allPatrons.Values] }, player.Channel);
    }

    public SharedRMCPatronFull? GetConnectedPatron(ICommonSession player)
    {
        return GetConnectedPatron(player.UserId);
    }

    public SharedRMCPatronFull? GetConnectedPatron(NetUserId userId)
    {
        return _connected.GetValueOrDefault(userId);
    }

    public bool TryGetPatron(NetUserId userId, out SharedRMCPatron? tier)
    {
        return _allPatrons.TryGetValue(userId, out tier);
    }

    public IReadOnlySet<Guid> GetFigurines()
    {
        return _figurines;
    }

    public string GetPatronOOCHexColor(NetUserId userId)
    {
        return "#FFFFFF";
    }

    private async Task RefreshAndSend(NetUserId userId)
    {
        await RefreshConnected(userId, CancellationToken.None);
        SendPatronStatus(userId);
    }

    private async Task RunLogged(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to update Discord account data:\n{e}");
        }
    }

    private async Task RefreshConnected(NetUserId userId, CancellationToken cancel)
    {
        var linked = await _db.HasLinkedAccount(userId.UserId, cancel);
        var patron = await _db.GetPatron(userId.UserId, cancel);
        _connected[userId] = ToSharedPatronFull(linked, patron);
    }

    private static SharedRMCPatronFull ToSharedPatronFull(bool linked, RMCPatron? patron)
    {
        var tier = patron?.Tier == null
            ? null
            : new SharedRMCPatronTier(
                patron.Tier.ShowOnCredits,
                patron.Tier.GhostColor,
                patron.Tier.NamedItems,
                patron.Tier.Figurines,
                patron.Tier.LobbyMessage,
                patron.Tier.RoundEndShoutout,
                patron.Tier.Name);

        Color? ghostColor = patron?.GhostColor == null
            ? null
            : System.Drawing.Color.FromArgb(patron.GhostColor.Value);

        var lobbyMessage = patron?.LobbyMessage?.Message == null
            ? null
            : new SharedRMCLobbyMessage(patron.LobbyMessage.Message);

        SharedRMCRoundEndShoutouts? shoutouts = null;
        if (patron?.RoundEndMarineShoutout != null || patron?.RoundEndXenoShoutout != null)
        {
            shoutouts = new SharedRMCRoundEndShoutouts(
                patron.RoundEndMarineShoutout?.Name,
                patron.RoundEndXenoShoutout?.Name);
        }

        return new SharedRMCPatronFull(tier, linked, ghostColor, lobbyMessage, shoutouts);
    }

    private static SharedRMCPatron ToSharedPatron(RMCPatron patron)
    {
        return new SharedRMCPatron(patron.Player.LastSeenUserName, patron.Tier.Name);
    }

    private string BuildDiscordOAuthLink(RMCDiscordOAuthLinkRequestMsg message)
    {
        var baseUrl = _config.GetCVar(RMCCVars.RMCDiscordOAuthBaseUrl).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException($"{RMCCVars.RMCDiscordOAuthBaseUrl.Name} is not configured.");

        var secret = _config.GetCVar(RMCCVars.RMCDiscordOAuthStateSecret);
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException($"{RMCCVars.RMCDiscordOAuthStateSecret.Name} is not configured.");

        var lifetime = Math.Max(30, _config.GetCVar(RMCCVars.RMCDiscordOAuthStateLifetimeSeconds));
        var locale = _config.GetClientCVar(message.MsgChannel, CCVars.ClientLocale);
        var payload = new DiscordOAuthStatePayload(
            message.MsgChannel.UserId.UserId,
            DateTimeOffset.UtcNow.AddSeconds(lifetime).ToUnixTimeSeconds(),
            Base64UrlEncode(RandomNumberGenerator.GetBytes(16)),
            locale);

        var payloadJson = JsonSerializer.Serialize(payload, StateJsonOptions);
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        byte[] signature;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
        }

        var state = $"{payloadBase64}.{Base64UrlEncode(signature)}";
        return $"{baseUrl}/auth/login?state={Uri.EscapeDataString(state)}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed record DiscordOAuthStatePayload(Guid PlayerId, long Expires, string Nonce, string Locale);

    void IPostInjectInit.PostInject()
    {
        _sawmill = _log.GetSawmill("rmc.link_account");
        _net.RegisterNetMessage<LinkAccountRequestMsg>(OnRequest);
        _net.RegisterNetMessage<LinkAccountCodeMsg>();
        _net.RegisterNetMessage<LinkAccountStatusMsg>();
        _net.RegisterNetMessage<RMCDiscordOAuthLinkRequestMsg>(OnDiscordOAuthLinkRequest);
        _net.RegisterNetMessage<RMCDiscordOAuthLinkMsg>();
        _net.RegisterNetMessage<RMCPatronListMsg>();
        _net.RegisterNetMessage<RMCClearGhostColorMsg>(OnClearGhostColor);
        _net.RegisterNetMessage<RMCChangeGhostColorMsg>(OnChangeGhostColor);
        _net.RegisterNetMessage<RMCChangeLobbyMessageMsg>(OnChangeLobbyMessage);
        _net.RegisterNetMessage<RMCChangeMarineShoutoutMsg>(OnChangeMarineShoutout);
        _net.RegisterNetMessage<RMCChangeXenoShoutoutMsg>(OnChangeXenoShoutout);
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnFinishLoad(FinishLoad);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
