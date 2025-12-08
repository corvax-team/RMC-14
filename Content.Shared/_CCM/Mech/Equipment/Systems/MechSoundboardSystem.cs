using System.Linq;
using Content.Shared._CCM.Mech.Components;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._CCM.Mech.Systems;

/// <summary>
/// Handles everything for mech soundboard.
/// </summary>
public sealed class MechSoundboardSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMMechSoundboardComponent, MechEquipmentUiStateReadyEvent>(OnUiStateReady);
        SubscribeLocalEvent<CCMMechSoundboardComponent, MechEquipmentUiMessageRelayEvent>(OnSoundboardMessage);
    }

    private void OnUiStateReady(EntityUid uid, CCMMechSoundboardComponent comp, MechEquipmentUiStateReadyEvent args)
    {
        // you have to specify a collection so it must exist probably
        var sounds = comp.Sounds.Select(sound => sound.Collection!);
        var state = new MechSoundboardUiState
        {
            Sounds = sounds.ToList()
        };
        args.States.Add(GetNetEntity(uid), state);
    }

    private void OnSoundboardMessage(EntityUid uid, CCMMechSoundboardComponent comp, MechEquipmentUiMessageRelayEvent args)
    {
        if (args.Message is not MechSoundboardPlayMessage msg)
            return;

        if (msg.Sound >= comp.Sounds.Count)
            return;

        if (TryComp(uid, out UseDelayComponent? useDelay)
            && !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        // honk!!!!!
        _audio.PlayPvs(comp.Sounds[msg.Sound], uid);
    }
}
