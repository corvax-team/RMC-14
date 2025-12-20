using System.Threading;
using Content.Client._Forge.DiscordAuth.DiscordGui;
using Content.Shared._Forge.DiscordAuth;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Client;
using Robust.Shared.Enums;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._Forge.DiscordAuth;

public sealed class DiscordAuthState : State
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IBaseClient _baseClient = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;

    private DiscordAuthGui _gui = default!;
    private readonly CancellationTokenSource _checkTimerCancel = new();

    private bool _authCompleted = false;

    protected override void Startup()
    {
        _gui = new DiscordAuthGui();
        _userInterfaceManager.StateRoot.AddChild(_gui);

        // Периодическая проверка авторизации
        Timer.SpawnRepeating(TimeSpan.FromSeconds(5),
            () => _netManager.ClientSendMessage(new MsgDiscordAuthCheck()),
            _checkTimerCancel.Token);

        // Подписка на смену RunLevel
        _baseClient.RunLevelChanged += OnRunLevelChanged;
    }

    private void OnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
    {
        // Если клиент перешёл в лобби/игру, но авторизация не завершена — возвращаем состояние DiscordAuth
        if (!_authCompleted && e.NewLevel == ClientRunLevel.InGame)
        {
            Logger.Debug("DiscordAuth: preventing premature transition to InGame before auth completes.");
            _stateManager.RequestStateChange<DiscordAuthState>();
        }
    }

    public void CompleteAuth()
    {
        _authCompleted = true;
        _checkTimerCancel.Cancel();

        if (_userInterfaceManager.StateRoot.Children.Contains(_gui))
            _userInterfaceManager.StateRoot.RemoveChild(_gui);

        Logger.Info("DiscordAuth: Authorization completed successfully.");
    }

    protected override void Shutdown()
    {
        _baseClient.RunLevelChanged -= OnRunLevelChanged;
        _checkTimerCancel.Cancel();

        if (_userInterfaceManager.StateRoot.Children.Contains(_gui))
            _userInterfaceManager.StateRoot.RemoveChild(_gui);
    }
}
