using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._RMC14.LinkAccount;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountManager : IPostInjectInit
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, TimeSpan> _lastRequest = new();
    private readonly TimeSpan _minimumWait = TimeSpan.FromSeconds(0.5);
    private readonly Dictionary<NetUserId, SharedRMCPatronFull> _connected = new();
    private readonly Dictionary<NetUserId, SharedRMCPatron> _allPatrons = [];
    private readonly HashSet<Guid> _figurines = [];

    public event Action? PatronsReloaded;
    public event Action<(NetUserId Id, SharedRMCPatronFull Patron)>? PatronUpdated;

    private async Task LoadData(ICommonSession player, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        _connected[player.UserId] = new SharedRMCPatronFull(null, false, null, null, null);
        await Task.CompletedTask;
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
        var time = _timing.RealTime;
        if (_lastRequest.TryGetValue(message.MsgChannel.UserId, out var last) &&
            last + _minimumWait > time)
        {
            return;
        }

        _lastRequest[message.MsgChannel.UserId] = time;
        _net.ServerSendMessage(new LinkAccountCodeMsg { Code = Guid.Empty }, message.MsgChannel);
    }

    private void OnClearGhostColor(RMCClearGhostColorMsg message)
    {
    }

    private void OnChangeGhostColor(RMCChangeGhostColorMsg message)
    {
    }

    private void OnChangeLobbyMessage(RMCChangeLobbyMessageMsg message)
    {
    }

    private void OnChangeMarineShoutout(RMCChangeMarineShoutoutMsg message)
    {
    }

    private void OnChangeXenoShoutout(RMCChangeXenoShoutoutMsg message)
    {
    }

    public async Task RefreshAllPatrons()
    {
        _allPatrons.Clear();
        _figurines.Clear();
        PatronsReloaded?.Invoke();
        await Task.CompletedTask;
    }

    public void SendPatronsToAll()
    {
        _net.ServerSendToAll(new RMCPatronListMsg { Patrons = [] });
    }

    private void SendPatrons(ICommonSession player)
    {
        _net.ServerSendMessage(new RMCPatronListMsg { Patrons = [] }, player.Channel);
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

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<LinkAccountRequestMsg>(OnRequest);
        _net.RegisterNetMessage<LinkAccountCodeMsg>();
        _net.RegisterNetMessage<LinkAccountStatusMsg>();
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
