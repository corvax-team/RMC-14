using Content.Server.Radio;
using Content.Shared._CCM.Silencer;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._CCM.Silencer;

/// <summary>
///     Cuts xenos hit by the sonic carbine off from the hivemind channel.
/// </summary>
public sealed class CCMHiveDisruptionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly TimeSpan PopupCooldown = TimeSpan.FromSeconds(1.5);
    private readonly Dictionary<EntityUid, TimeSpan> _nextPopup = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMHiveDisruptedComponent, RadioSendAttemptEvent>(OnSendAttempt);
        SubscribeLocalEvent<CCMHiveDisruptedComponent, RadioReceiveAttemptEvent>(OnReceiveAttempt);
        SubscribeLocalEvent<CCMHiveDisruptedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSendAttempt(Entity<CCMHiveDisruptedComponent> ent, ref RadioSendAttemptEvent args)
    {
        if (args.Channel.ID != SharedChatSystem.HivemindChannel.Id)
            return;

        args.Cancelled = true;

        var now = _timing.CurTime;
        if (_nextPopup.TryGetValue(ent, out var next) && now < next)
            return;

        _nextPopup[ent] = now + PopupCooldown;
        _popup.PopupEntity(Loc.GetString("ccm-silencer-hive-cant-speak"), ent, ent, PopupType.MediumCaution);
    }

    private void OnReceiveAttempt(Entity<CCMHiveDisruptedComponent> ent, ref RadioReceiveAttemptEvent args)
    {
        if (args.Channel.ID != SharedChatSystem.HivemindChannel.Id)
            return;

        args.Cancelled = true;
    }

    private void OnShutdown(Entity<CCMHiveDisruptedComponent> ent, ref ComponentShutdown args)
    {
        _nextPopup.Remove(ent);
    }
}
