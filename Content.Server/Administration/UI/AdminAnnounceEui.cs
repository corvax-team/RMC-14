using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Shared.Audio;
using Robust.Shared.ContentPack;

namespace Content.Server.Administration.UI
{
    public sealed class AdminAnnounceEui : BaseEui
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        
        private readonly ChatSystem _chatSystem;

        public AdminAnnounceEui()
        {
            IoCManager.InjectDependencies(this);
            _chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new AdminAnnounceEuiState();
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case AdminAnnounceEuiMsg.DoAnnounce doAnnounce:
                    if (!_adminManager.HasAdminFlag(Player, AdminFlags.Admin))
                    {
                        Close();
                        break;
                    }
                    // CCM14-start
                    var hex = "#" + (doAnnounce.ColorHex?.Trim().TrimStart('#') ?? "1d8bad");
                    Color color;
                    try
                    {
                        color = Color.FromHex(hex);
                    }
                    catch
                    {
                        color = Color.FromHex("#1d8bad");
                    }

                    SoundSpecifier? sound = null;
                    if (!string.IsNullOrWhiteSpace(doAnnounce.SoundPath) && _resourceManager.ContentFileExists(doAnnounce.SoundPath))
                        sound = new SoundPathSpecifier(doAnnounce.SoundPath);
                    // CCM14-end
                    switch (doAnnounce.AnnounceType)
                    {
                        case AdminAnnounceType.Server:
                            _chatManager.DispatchServerAnnouncement(doAnnounce.Announcement, color); // CCM14
                            break;
                        // TODO: Per-station announcement support
                        case AdminAnnounceType.Station:
                            // CCM14-start
                            _chatSystem.DispatchGlobalAnnouncement(
                                doAnnounce.Announcement +
                                (!string.IsNullOrWhiteSpace(doAnnounce.Sender)
                                    ? $"\n{Loc.GetString("comms-console-announcement-sent-by")} {doAnnounce.Sender}"
                                    : ""),
                                string.IsNullOrWhiteSpace(doAnnounce.Announcer)
                                    ? Loc.GetString("chat-manager-sender-announcement")
                                    : doAnnounce.Announcer,
                                colorOverride: color,
                                playSound: true,
                                announcementSound: sound
                            );
                            // CCM14-end
                            break;
                    }

                    StateDirty();

                    if (doAnnounce.CloseAfter)
                        Close();

                    break;
            }
        }
    }
}
