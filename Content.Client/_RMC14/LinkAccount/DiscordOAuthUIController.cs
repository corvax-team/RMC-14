using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._RMC14.LinkAccount;

public sealed class DiscordOAuthUIController : UIController
{
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;

    public override void Initialize()
    {
        _linkAccount.OAuthLinkReceived += OnOAuthLinkReceived;
    }

    public void OpenLink()
    {
        _linkAccount.RequestDiscordOAuthLink();
    }

    private void OnOAuthLinkReceived(string url, string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Log.Warning($"Unable to open Discord OAuth link: {error}");
            return;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            Log.Warning("Unable to open Discord OAuth link: server returned an empty URL.");
            return;
        }

        _uri.OpenUri(url);
    }
}
