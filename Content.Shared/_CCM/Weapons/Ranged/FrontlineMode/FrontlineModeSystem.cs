using System.Numerics;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Actions;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Physics;
using Content.Shared.Physics;

namespace Content.Shared._CCM.Weapons.Ranged.Frontline;

public sealed class SmartGunFrontlineSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly GunIFFSystem _gunIFF = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<GunIFFComponent> _iffQuery;

    private readonly List<RayCastResults> _raycastResults = new();
    private readonly HashSet<EntProtoId<IFFFactionComponent>> _shooterFactions = new();
    private readonly HashSet<EntProtoId<IFFFactionComponent>> _targetFactions = new();

    private static readonly Comparison<RayCastResults> RaycastDistanceComparison =
        static (a, b) => a.Distance.CompareTo(b.Distance);

    public override void Initialize()
    {
        _iffQuery = GetEntityQuery<GunIFFComponent>();

        SubscribeLocalEvent<SmartGunFrontlineComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<SmartGunFrontlineComponent, GunToggleFrontlineActionEvent>(OnToggleAction);
        SubscribeLocalEvent<SmartGunFrontlineComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<SmartGunFrontlineComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnGetItemActions(Entity<SmartGunFrontlineComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.Action, ent.Comp.ActionId);
        Dirty(ent);
    }

    private void OnToggleAction(Entity<SmartGunFrontlineComponent> ent, ref GunToggleFrontlineActionEvent args)
    {
        args.Handled = true;
        ent.Comp.Enabled = !ent.Comp.Enabled;
        ToggleFrontline(ent, args.Performer);
    }

    private void ToggleFrontline(Entity<SmartGunFrontlineComponent> ent, EntityUid user)
    {
        _actions.SetToggled(ent.Comp.Action, ent.Comp.Enabled);

        var key = ent.Comp.Enabled ? "ccm-smartgun-frontline-enabled" : "ccm-smartgun-frontline-disabled";
        var message = Loc.GetString(key);
        _popup.PopupClient(message, user, user, PopupType.Large);

        _audio.PlayPredicted(ent.Comp.ToggleSound, ent, user);
        Dirty(ent);
    }

    private void OnAmmoShot(Entity<SmartGunFrontlineComponent> ent, ref AmmoShotEvent args)
    {
        var useAltFalloff = ent.Comp.Enabled;

        if (!useAltFalloff && _iffQuery.TryComp(ent, out var iff))
            useAltFalloff = !iff.Enabled;

        if (!useAltFalloff)
            return;

        foreach (var projectile in args.FiredProjectiles)
        {
            if (TryComp<RMCProjectileDamageFalloffComponent>(projectile, out var falloff))
            {
                falloff.Thresholds = ent.Comp.AltFalloffThresholds;
                Dirty(projectile, falloff);
            }
        }
    }

    private void OnShotAttempted(Entity<SmartGunFrontlineComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!ent.Comp.Enabled || args.Cancelled)
            return;

        if (!_iffQuery.TryComp(ent, out var iff) || !iff.Enabled)
            return;

        var shooter = args.User;
        var shooterXform = Transform(shooter);
        var gunXform = Transform(ent.Owner);

        var gunPos = _transform.GetWorldPosition(gunXform);
        var targetPos = GetTargetPosition(ent, shooter, shooterXform, gunPos);
        if (float.IsNaN(targetPos.X) || float.IsNaN(targetPos.Y) ||
            float.IsInfinity(targetPos.X) || float.IsInfinity(targetPos.Y))
            return;

        var direction = targetPos - gunPos;
        if (direction.LengthSquared() < 0.001f)
            return;

        direction = Vector2.Normalize(direction);

        _shooterFactions.Clear();
        var shooterUserIFF = CompOrNull<UserIFFComponent>(shooter);
        if (!_gunIFF.TryGetFactions((shooter, shooterUserIFF), _shooterFactions, SlotFlags.IDCARD))
            return;

        var ray = new CollisionRay(gunPos, direction, (int)CollisionGroup.AllMask);
        _raycastResults.Clear();

        foreach (var hit in _physics.IntersectRay(gunXform.MapID, ray, ent.Comp.MaxDistance, shooter, false))
        {
            _raycastResults.Add(hit);
        }

        if (_raycastResults.Count == 0)
            return;

        _raycastResults.Sort(RaycastDistanceComparison);

        foreach (var hit in _raycastResults)
        {
            if (hit.HitEntity == shooter || hit.HitEntity == ent.Owner)
                continue;

            if (CheckFactions(hit.HitEntity))
            {
                args.Cancel();
                ShowBlockedMessage(ent, shooter);
                return;
            }
        }
    }

    private Vector2 GetTargetPosition(
        Entity<SmartGunFrontlineComponent> ent,
        EntityUid shooter,
        TransformComponent shooterXform,
        Vector2 gunPos)
    {
        if (TryComp<GunComponent>(ent, out var gunComp))
        {
            if (gunComp.ShootCoordinates is { } shootCoord)
            {
                var targetMap = _transform.ToMapCoordinates(shootCoord);
                return targetMap.Position;
            }

            if (gunComp.Target is { } targetEnt && targetEnt != EntityUid.Invalid)
            {
                var targetXform = Transform(targetEnt);
                return _transform.GetWorldPosition(targetXform);
            }
        }

        var shooterRot = _transform.GetWorldRotation(shooterXform);
        var aimDir = shooterRot.ToVec();
        return gunPos + aimDir * ent.Comp.MaxDistance;
    }

    private bool CheckFactions(EntityUid target)
    {
        _targetFactions.Clear();
        var targetUserIFF = CompOrNull<UserIFFComponent>(target);
        bool gotFactions = _gunIFF.TryGetFactions((target, targetUserIFF), _targetFactions, SlotFlags.IDCARD);

        if (!gotFactions)
        {
            _targetFactions.Clear();
            gotFactions = _gunIFF.TryGetFactions((target, targetUserIFF), _targetFactions);
        }

        if (!gotFactions)
        {
            var parent = Transform(target).ParentUid;
            if (parent != EntityUid.Invalid)
            {
                var parentUserIFF = CompOrNull<UserIFFComponent>(parent);

                _targetFactions.Clear();
                gotFactions = _gunIFF.TryGetFactions((parent, parentUserIFF), _targetFactions, SlotFlags.IDCARD);

                if (!gotFactions)
                {
                    _targetFactions.Clear();
                    gotFactions = _gunIFF.TryGetFactions((parent, parentUserIFF), _targetFactions);
                }
            }
        }

        return gotFactions && _shooterFactions.Overlaps(_targetFactions);
    }

    private void ShowBlockedMessage(Entity<SmartGunFrontlineComponent> ent, EntityUid shooter)
    {
        var currentTime = _timing.CurTime;
        if (currentTime >= ent.Comp.NextBlockMessageTime)
        {
            _popup.PopupClient(
                Loc.GetString(ent.Comp.BlockedMessage),
                shooter,
                shooter,
                PopupType.MediumCaution
            );
            _audio.PlayPredicted(ent.Comp.BlockSound, ent, shooter);
            ent.Comp.NextBlockMessageTime = currentTime + ent.Comp.BlockMessageCooldown;
            Dirty(ent);
        }
    }
}
