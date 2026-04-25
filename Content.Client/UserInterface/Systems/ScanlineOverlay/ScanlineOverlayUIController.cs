using System.Collections.Generic;
using System.Numerics;
using Content.Client.Lobby.UI;
using Content.Client.Lobby;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.ScanlineOverlay;

public sealed class ScanlineOverlayUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public void OnStateEntered(LobbyState state)
    {
    }

    public void OnStateExited(LobbyState state)
    {
    }
}
