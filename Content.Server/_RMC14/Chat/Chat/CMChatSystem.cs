using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Chat.Managers;
using Content.Server.Radio.Components;
using Content.Server.Speech.EntitySystems;
using Content.Server.Speech.Prototypes;
using Content.Shared._CMU14.Yautja;
using Content.Shared._RMC14.Chat;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Mentor.ImaginaryFriend;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Chat;
using Content.Shared.Ghost;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Content.Server.Chat.Systems;
using Content.Shared.Speech;

namespace Content.Server._RMC14.Chat.Chat;

public sealed class CMChatSystem : SharedCMChatSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _wordreplacement = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static readonly ProtoId<ReplacementAccentPrototype> ChatSanitize = "CMChatSanitize";
    private static readonly ProtoId<ReplacementAccentPrototype> MarineChatSanitize = "CMChatSanitizeMarine";
    private static readonly ProtoId<ReplacementAccentPrototype> XenoChatSanitize = "CMChatSanitizeXeno";

    private static readonly Regex PrefixesRegex = new(@"^:(\w)+");

    private readonly List<ICommonSession> _toRemove = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MarineComponent, ChatMessageAfterGetRecipients>(OnMarineAfterGetRecipients);
        SubscribeLocalEvent<XenoComponent, ChatMessageAfterGetRecipients>(OnXenoAfterGetRecipients);
        SubscribeLocalEvent<ImaginaryFriendComponent, ChatMessageAfterGetRecipients>(OnImaginaryFriendGetRecipients);
    }

    private void OnMarineAfterGetRecipients(Entity<MarineComponent> ent, ref ChatMessageAfterGetRecipients args)
    {
        _toRemove.Clear();

        foreach (var (session, data) in args.Recipients)
        {
            if (data.Observer)
                continue;

            if (session.AttachedEntity is { } attached &&
                HasComp<XenoComponent>(attached) &&
                !IsHivebrokenXeno(attached))
            {
                _toRemove.Add(session);
            }
        }

        foreach (var session in _toRemove)
        {
            args.Recipients.Remove(session);
        }
    }

    private void OnXenoAfterGetRecipients(Entity<XenoComponent> ent, ref ChatMessageAfterGetRecipients args)
    {
        _toRemove.Clear();

        if (!IsHivebrokenXeno(ent.Owner))
        {
            foreach (var (session, data) in args.Recipients)
            {
                if (data.Observer)
                    continue;

            // `data.Observer` only indicates whether the recipient has `GhostHearingComponent`.
            // Disabling ghost hearing removes this component, so the `GhostComponent` check is needed to keep ghosts included.
            if (!HasComp<XenoComponent>(session.AttachedEntity) && !HasComp<GhostComponent>(session.AttachedEntity))
                _toRemove.Add(session);
        }

            foreach (var session in _toRemove)
            {
                args.Recipients.Remove(session);
            }
        }
    }

    private void OnImaginaryFriendGetRecipients(Entity<ImaginaryFriendComponent> ent, ref ChatMessageAfterGetRecipients args)
    {
        _toRemove.Clear();

        foreach (var (session, data) in args.Recipients)
        {
            if (data.Observer)
                continue;

            if (ent.Comp.Imaginer != session.AttachedEntity)
                _toRemove.Add(session);
        }

        foreach (var session in _toRemove)
        {
            args.Recipients.Remove(session);
        }
    }

    public override string SanitizeMessageReplaceWords(EntityUid source, string msg)
    {
        msg = _wordreplacement.ApplyReplacements(msg, ChatSanitize);

        var factionSanitize = HasComp<XenoComponent>(source) && !IsHivebrokenXeno(source)
            ? XenoChatSanitize
            : MarineChatSanitize;
        msg = _wordreplacement.ApplyReplacements(msg, factionSanitize);

        return msg;
    }

    public override void ChatMessageToOne(
        ChatChannel channel,
        string message,
        string wrappedMessage,
        EntityUid source,
        bool hideChat,
        INetChannel client,
        Color? colorOverride = null,
        bool recordReplay = false,
        string? audioPath = null,
        float audioVolume = 0,
        NetUserId? author = null)
    {
        _chat.ChatMessageToOne(
            channel,
            message,
            wrappedMessage,
            source,
            hideChat,
            client,
            colorOverride,
            recordReplay,
            audioPath,
            audioVolume,
            author
        );
    }

    public override void ChatMessageToMany(
        string message,
        string wrappedMessage,
        Filter filter,
        ChatChannel channel,
        EntityUid source = default,
        bool hideChat = false,
        Color? colorOverride = null,
        bool recordReplay = false,
        string? audioPath = null,
        float audioVolume = 0,
        NetUserId? author = null)
    {
        if (channel == ChatChannel.Radio && source != default)
        {
            ChatMessageToManyRadio(message, wrappedMessage, filter, source, hideChat, colorOverride, recordReplay, audioPath, audioVolume, author);
            return;
        }

        _chat.ChatMessageToManyFiltered(
            filter,
            channel,
            message,
            wrappedMessage,
            source,
            hideChat,
            recordReplay,
            colorOverride,
            audioPath,
            audioVolume
        );
    }

    private bool IsValidRadioPrefix(EntityUid headset, string prefixPart)
    {
        if (prefixPart.Length != 2)
            return false;

        if (!TryComp(headset, out EncryptionKeyHolderComponent? keys))
            return false;

        var prefix = prefixPart[0];
        if (prefix == SharedChatSystem.RadioChannelAltPrefix)
            prefix = SharedChatSystem.RadioChannelPrefix;

        var keycode = char.ToLowerInvariant(prefixPart[1]);

        if (keycode == SharedChatSystem.DefaultChannelKey && keys.DefaultChannel != null)
            return true;

        foreach (var ch in _proto.EnumeratePrototypes<RadioChannelPrototype>())
        {
            if (!keys.Channels.Contains(ch.ID))
                continue;

            if (ch.RadioPrefix == prefix && ch.KeyCode == keycode)
                return true;
        }

        return false;
    }

    private bool IsValidRadioKey(EntityUid headset, char prefix, char keycode)
    {
        return IsValidRadioPrefix(headset, $"{prefix}{char.ToLowerInvariant(keycode)}");
    }

    // cursed code
    private void ChatMessageToManyRadio(
        string message,
        string wrappedMessage,
        Filter filter,
        EntityUid source,
        bool hideChat = false,
        Color? colorOverride = null,
        bool recordReplay = false,
        string? audioPath = null,
        float audioVolume = 0,
        NetUserId? author = null)
    {
        var hivemindMessage = $";{message}";

        if (!_chatSystem.TryProccessRadioMessage(source, hivemindMessage, out var processedMessage, out var channel))
            return;

        var transformSpeakerEv = new TransformSpeakerNameEvent(source, Name(source));
        RaiseLocalEvent(source, transformSpeakerEv);

        var speechVerb = _chatSystem.GetSpeechVerb(source, processedMessage);
        var sanitizedMessage = SanitizeMessageReplaceWords(source, processedMessage);

        var finalColor = colorOverride ?? channel?.Color ?? Color.White;
        var finalWrappedMessage = FormatRadioMessage(transformSpeakerEv.VoiceName, sanitizedMessage, speechVerb, channel);

        _chat.ChatMessageToManyFiltered(
            filter,
            ChatChannel.Radio,
            sanitizedMessage,
            finalWrappedMessage,
            source,
            hideChat,
            recordReplay,
            finalColor,
            audioPath,
            audioVolume
        );
    }

    private string FormatRadioMessage(string speakerName, string message, SpeechVerbPrototype speechVerb, RadioChannelPrototype? channel)
    {
        var channelName = channel?.Name ?? "Unknown";
        // CCM14-start
        var verbId = speechVerb.SpeechVerbStrings.Count > 0
            ? _random.Pick(speechVerb.SpeechVerbStrings)
            : "chat-speech-verb-default";
        var verb = Loc.GetString(verbId);
        // CCM14-end
        var channelColor = channel?.Color.ToHex() ?? "#FFFFFF";

        var formattedMessage = $"[color={channelColor}][bold]\\[{Loc.GetString(channelName)}\\][/bold][/color] [bold]{speakerName}[/bold] {verb}, \"{message}\"";

        if (speechVerb.Bold)
            formattedMessage = $"[bold]{formattedMessage}[/bold]";

        if (speechVerb.FontSize != 12)
            formattedMessage = $"[font size={speechVerb.FontSize}]{formattedMessage}[/font]";

        return formattedMessage;
    }

    public List<string>? TryMultiBroadcast(EntityUid source, string message)
    {
        if (string.IsNullOrEmpty(message) || message.Length < 2)
            return null;

        if (!HasComp<InventoryComponent>(source))
            return null;

        var time = _timing.CurTime;
        Entity<HeadsetMultiBroadcastComponent>? headset = null;
        var ears = _inventory.GetSlotEnumerator(source, SlotFlags.EARS);
        while (ears.MoveNext(out var ear))
        {
            if (ear.ContainedEntity is not { } contained)
                continue;

            if (TryComp(contained, out HeadsetMultiBroadcastComponent? headsetComp))
            {
                headset = (contained, headsetComp);
                break;
            }
        }

        if (headset == null)
            return null;

        var validPrefixes = new List<string>();
        var prefixLength = 0;
        var sharedPrefix = message[0];

        if (sharedPrefix != SharedChatSystem.RadioChannelPrefix &&
            sharedPrefix != SharedChatSystem.RadioChannelAltPrefix)
            return null;

        for (var i = 1; i < message.Length; i++)
        {
            var keycode = char.ToLowerInvariant(message[i]);
            if (char.IsWhiteSpace(keycode))
            {
                prefixLength = i;
                break;
            }

            if (!IsValidRadioKey(headset.Value, sharedPrefix, keycode))
            {
                prefixLength = i;
                break;
            }

            validPrefixes.Add($"{sharedPrefix}{keycode}");
            prefixLength = i + 1;
        }

        var count = Math.Min(validPrefixes.Count, headset.Value.Comp.Maximum);
        validPrefixes = validPrefixes.Take(count).ToList();

        if (validPrefixes.Count < 2)
            return null;

        var messages = new List<string>(validPrefixes.Count);
        var messageBody = message[prefixLength..];

        for (var idx = 0; idx < validPrefixes.Count; idx++)
            messages.Add($"{validPrefixes[idx]}{messageBody}");

        if (messages.Count < 2)
            return null;

        var timeLeft = headset.Value.Comp.Last + headset.Value.Comp.Cooldown - time;
        if (headset.Value.Comp.Last != null &&
            timeLeft != null &&
            timeLeft.Value > TimeSpan.Zero)
        {
            _popup.PopupEntity(
                $"You've used the multi-broadcast system too recently, wait {timeLeft.Value.TotalSeconds:F0} more seconds.",
                source,
                source,
                PopupType.MediumCaution
            );

            messages.Clear();
            return messages;
        }

        headset.Value.Comp.Last = time;
        Dirty(headset.Value);
        return messages;
    }
}
