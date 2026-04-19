// CM14 rework: non-RMC edit marker.
using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Shared._CCM.Barks;
using Content.Shared.CCVar;
using System.Linq;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CCM.Barks;

public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<ActorComponent, HeadsetRadioReceiveRelayEvent>(OnHeadsetRadioReceive);
        SubscribeNetworkEvent<RequestPreviewBarkEvent>(OnPreviewRequest);
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        if (!_cfg.GetCVar(CCVars.BarksEnabled))
            return;

        if (string.IsNullOrWhiteSpace(args.Message))
            return;

        var synthesis = CompOrNull<SpeechSynthesisComponent>(args.Source);
        var bark = ResolveBarkPrototype(synthesis?.VoicePrototypeId?.Id);
        if (bark == null || bark.SoundFiles.Count == 0)
            return;

        var soundPath = _random.Pick(bark.SoundFiles);
        var pitch = Math.Clamp(synthesis?.Pitch ?? 1f, 0.7f, 1.4f);
        var speed = Math.Clamp(synthesis?.PlaybackSpeed ?? 1f, 0.7f, 1.4f);
        var isWhisper = args.ObfuscatedMessage != null;

        var ev = new PlayBarkEvent(
            soundPath,
            GetNetEntity(args.Source),
            args.Message,
            speed,
            pitch,
            isWhisper,
            isWhisper
        );

        RaiseNetworkEvent(ev);
    }

    private void OnHeadsetRadioReceive(Entity<ActorComponent> ent, ref HeadsetRadioReceiveRelayEvent args)
    {
        if (!_cfg.GetCVar(CCVars.BarksEnabled))
            return;

        var relayed = args.RelayedEvent;
        if (string.IsNullOrWhiteSpace(relayed.Message))
            return;

        if (relayed.MessageSource == ent.Owner)
            return;

        var synthesis = CompOrNull<SpeechSynthesisComponent>(relayed.MessageSource);
        var bark = ResolveBarkPrototype(synthesis?.VoicePrototypeId?.Id);
        if (bark == null || bark.SoundFiles.Count == 0)
            return;

        var soundPath = _random.Pick(bark.SoundFiles);
        var pitch = Math.Clamp(synthesis?.Pitch ?? 1f, 0.7f, 1.4f);
        var speed = Math.Clamp(synthesis?.PlaybackSpeed ?? 1f, 0.7f, 1.4f);

        var ev = new PlayBarkEvent(
            soundPath,
            GetNetEntity(relayed.MessageSource),
            relayed.Message,
            speed,
            pitch,
            false,
            false,
            fromRadio: true
        );

        RaiseNetworkEvent(ev, ent.Comp.PlayerSession);
    }

    private void OnPreviewRequest(RequestPreviewBarkEvent msg, EntitySessionEventArgs args)
    {
        if (!_cfg.GetCVar(CCVars.BarksEnabled))
            return;

        var bark = ResolveBarkPrototype(msg.BarkVoiceId);
        if (bark == null || bark.SoundFiles.Count == 0)
            return;

        var soundPath = _random.Pick(bark.SoundFiles);
        var pitch = Math.Clamp(msg.Pitch, 0.7f, 1.4f);
        var speed = Math.Clamp(msg.PlaybackSpeed, 0.7f, 1.4f);

        var sourceUid = args.SenderSession.AttachedEntity is { Valid: true } attached
            ? GetNetEntity(attached)
            : NetEntity.Invalid;

        var previewCount = _random.Next(3, 5);

        RaiseNetworkEvent(
            new PlayBarkEvent(soundPath, sourceUid, ".", speed, pitch, false, false, 0f, preview: true, previewCount: previewCount),
            args.SenderSession);
    }

    private BarkPrototype? ResolveBarkPrototype(string? id)
    {
        if (!string.IsNullOrWhiteSpace(id) &&
            _prototype.TryIndex<BarkPrototype>(id, out var selected))
        {
            return selected;
        }

        if (_prototype.TryIndex<BarkPrototype>(BarkPrototype.Default, out var fallback))
            return fallback;

        return _prototype.EnumeratePrototypes<BarkPrototype>().FirstOrDefault();
    }
}
