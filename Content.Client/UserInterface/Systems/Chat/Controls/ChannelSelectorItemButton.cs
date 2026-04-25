using Content.Client.Stylesheets;
using Content.Shared.Chat;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class ChannelSelectorItemButton : Button
{
    public readonly ChatSelectChannel Channel;

    public bool IsHidden => Parent == null;

    public ChannelSelectorItemButton(ChatSelectChannel selector)
    {
        Channel = selector;
        AddStyleClass(StyleNano.StyleClassChatChannelSelectorButton);
        RefreshLocalization();
    }

    // CCM rework lobby - start
    public void RefreshLocalization()
    {
        var text = ChannelSelectorButton.ChannelSelectorName(Channel);
        var prefix = ChatUIController.ChannelPrefixes[Channel];

        if (prefix != default)
            text = Loc.GetString("hud-chatbox-select-name-prefixed", ("name", text), ("prefix", prefix));

        Text = text;
    }
    // CCM rework lobby - end
}
