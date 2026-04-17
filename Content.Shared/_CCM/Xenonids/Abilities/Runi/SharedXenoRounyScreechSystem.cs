using Content.Shared._RMC14.Deafness;
using Content.Shared._RMC14.Xenonids.Screech;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Coordinates;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;

namespace Content.Shared._CCM14.Xenonids.Screech;

public sealed class XenoRounyScreechSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDeafnessSystem _deaf = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityManager _ent = default!;
    // [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    // [Dependency] private readonly EvasionSystem _evasion = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobs = new();
    private readonly HashSet<Entity<MobStateComponent>> _closeMobs = new();
    private readonly HashSet<Entity<XenoParasiteComponent>> _parasites = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoRounyScreechComponent, XenoRounyScreechActionEvent>(OnXenoScreechAction);
    }

    private void OnXenoScreechAction(Entity<XenoRounyScreechComponent> xeno, ref XenoRounyScreechActionEvent args)
    {
        if (args.Handled)
            return;

        var attempt = new XenoRounyScreechAttemptEvent();
        RaiseLocalEvent(xeno, ref attempt);

        if (attempt.Cancelled)
            return;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        if (!TryComp(xeno, out TransformComponent? xform))
            return;

        args.Handled = true;

        if (_net.IsServer)
            _audio.PlayPvs(xeno.Comp.Sound, xeno);

        // =========================
        // CLOSE RANGE (strong panic)
        // =========================
        _closeMobs.Clear();
        _entityLookup.GetEntitiesInRange(xform.Coordinates, xeno.Comp.ParalyzeRange, _closeMobs);

        foreach (var receiver in _closeMobs)
        {
            if (!_xeno.CanAbilityAttackTarget(xeno, receiver))
                continue;

            // ApplyConfusion(receiver);
            ApplyDeaf(xeno, receiver, xeno.Comp.CloseDeafTime);

            ApplyAccuracyDebuff(
                receiver,
                -10,
                TimeSpan.FromSeconds(8));
        }

        // =========================
        // FAR RANGE (medium panic)
        // =========================
        _mobs.Clear();
        _entityLookup.GetEntitiesInRange(xform.Coordinates, xeno.Comp.StunRange, _mobs);

        foreach (var receiver in _mobs)
        {
            if (!_xeno.CanAbilityAttackTarget(xeno, receiver))
                continue;

            if (_closeMobs.Contains(receiver))
                continue;

            // ApplyConfusion(receiver);
            ApplyDeaf(xeno, receiver, xeno.Comp.FarDeafTime);

            ApplyAccuracyDebuff(
                receiver,
                -10,
                TimeSpan.FromSeconds(5));
        }

        // =========================
        // PARASITES (max chaos)
        // =========================
        _parasites.Clear();
        _entityLookup.GetEntitiesInRange(xform.Coordinates, xeno.Comp.ParasiteStunRange, _parasites);

        // foreach (var receiver in _parasites)
        // {
        //     ApplyConfusion(receiver);
        // }

        if (_net.IsServer)
            SpawnAttachedTo(xeno.Comp.Effect, xeno.Owner.ToCoordinates());
    }

    // private void ApplyConfusion(EntityUid target)
    // {
    //     if (_mobState.IsDead(target))
    //         return;

    //     var comp = _ent.EnsureComponent<XenoScreechConfusionComponent>(target);

    //     comp.ExpireAt = _timing.CurTime + TimeSpan.FromSeconds(10);
    // }

    private void ApplyDeaf(EntityUid xeno, EntityUid receiver, TimeSpan time)
    {
        if (_mobState.IsDead(receiver))
            return;

        if (!_examineSystem.InRangeUnOccluded(xeno, receiver))
            return;

        _deaf.TryDeafen(receiver, time, false);
    }

    private void ApplyAccuracyDebuff(EntityUid target, int multiplier, TimeSpan duration)
    {
        if (_mobState.IsDead(target))
            return;

        var comp = _ent.EnsureComponent<XenoScreechAccuracyDebuffComponent>(target);
        

        var time = _timing.CurTime;
        comp.Received.Add((multiplier, time + duration));
        comp.Received.Sort((a, b) => a.CompareTo(b));
        
        comp.AccuracyModifier = -0.8f; // или -1f
        comp.AccuracyPerTileModifier = -10f;

        // важно: обновить системы, которые читают модификаторы
        // _movement.RefreshMovementSpeedModifiers(target);
        // _evasion.RefreshEvasionModifiers(target);

        // RaiseLocalEvent(target, ref new RefreshMovementModifiersEvent());
        // RaiseLocalEvent(target, ref new RefreshEvasionModifiersEvent());
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        RemoveExpired();
    }

    private void RemoveExpired()
    {
        var query = EntityQueryEnumerator<XenoScreechAccuracyDebuffComponent>();
        var time = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            for (var i = comp.Received.Count - 1; i >= 0; i--)
            {
                if (comp.Received[i].ExpiresAt < time)
                    comp.Received.RemoveAt(i);
            }

            if (comp.Received.Count == 0)
                RemCompDeferred<XenoScreechAccuracyDebuffComponent>(uid);
        }
    }
}