using System.Linq;
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
        var useAltFalloff = false;
        if (TryComp<GunIFFComponent>(ent, out var iff) && !iff.Enabled)
            useAltFalloff = true;
        else if (TryComp<SmartGunFrontlineComponent>(ent, out var frontline) && frontline.Enabled)
            useAltFalloff = true;

        if (useAltFalloff && TryComp<SmartGunFrontlineComponent>(ent, out var frontlineForFalloff))
        {
            foreach (var projectile in args.FiredProjectiles)
            {
                if (TryComp<RMCProjectileDamageFalloffComponent>(projectile, out var falloff))
                {
                    falloff.Thresholds = frontlineForFalloff.AltFalloffThresholds;
                    Dirty(projectile, falloff);
                }
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

        Vector2 targetPos;
        if (TryComp<GunComponent>(ent, out var gunComp))
        {
            if (gunComp.ShootCoordinates is { } shootCoord)
            {
                var targetMap = _transform.ToMapCoordinates(shootCoord);
                targetPos = targetMap.Position;
            }
            else if (gunComp.Target is { } targetEnt && targetEnt != EntityUid.Invalid)
            {
                var targetXform = Transform(targetEnt);
                targetPos = _transform.GetWorldPosition(targetXform);
            }
            else
            {
                var shooterRot = _transform.GetWorldRotation(shooterXform);
                var aimDir = shooterRot.ToVec();
                targetPos = gunPos + aimDir * ent.Comp.MaxDistance;
            }
        }
        else
        {
            var shooterRot = _transform.GetWorldRotation(shooterXform);
            var aimDir = shooterRot.ToVec();
            targetPos = gunPos + aimDir * ent.Comp.MaxDistance;
        }

        var direction = targetPos - gunPos;
        if (direction.LengthSquared() < 0.001f)
            return;

        if (float.IsNaN(direction.X) || float.IsNaN(direction.Y) ||
            float.IsInfinity(direction.X) || float.IsInfinity(direction.Y))
            return;

        direction = Vector2.Normalize(direction);

        var factions = new HashSet<EntProtoId<IFFFactionComponent>>();
        var shooterUserIFF = CompOrNull<UserIFFComponent>(shooter);
        if (!_gunIFF.TryGetFactions((shooter, shooterUserIFF), factions, SlotFlags.IDCARD))
            return;

        var ray = new CollisionRay(gunPos, direction, (int)CollisionGroup.AllMask);
        var results = _physics.IntersectRay(gunXform.MapID, ray, ent.Comp.MaxDistance, shooter, false).ToList();

        foreach (var hit in results.OrderBy(r => r.Distance))
        {
            if (hit.HitEntity == shooter || hit.HitEntity == ent.Owner)
                continue;

            if (CheckFactions(hit.HitEntity, factions))
            {
                args.Cancel();
                ShowBlockedMessage(ent, shooter);
                return;
            }
        }
    }

    private bool CheckFactions(EntityUid target, HashSet<EntProtoId<IFFFactionComponent>> shooterFactions)
    {
        var targetFactions = new HashSet<EntProtoId<IFFFactionComponent>>();

        var targetUserIFF = CompOrNull<UserIFFComponent>(target);
        bool gotFactions = _gunIFF.TryGetFactions((target, targetUserIFF), targetFactions, SlotFlags.IDCARD);
        if (!gotFactions)
            gotFactions = _gunIFF.TryGetFactions((target, targetUserIFF), targetFactions);

        if (!gotFactions)
        {
            var parent = Transform(target).ParentUid;
            if (parent != EntityUid.Invalid)
            {
                var parentUserIFF = CompOrNull<UserIFFComponent>(parent);
                gotFactions = _gunIFF.TryGetFactions((parent, parentUserIFF), targetFactions, SlotFlags.IDCARD);
                if (!gotFactions)
                    gotFactions = _gunIFF.TryGetFactions((parent, parentUserIFF), targetFactions);
            }
        }

        return gotFactions && shooterFactions.Overlaps(targetFactions);
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
