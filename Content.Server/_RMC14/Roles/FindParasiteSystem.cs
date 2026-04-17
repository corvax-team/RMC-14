using Content.Server._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.Roles.FindParasite;
using Content.Shared._RMC14.Xenonids.Construction.EggMorpher;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Content.Shared.Coordinates;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Content.Shared.UserInterface;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Network;

namespace Content.Server._RMC14.Roles;

public sealed partial class FindParasiteSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entities = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly AreaSystem _areas = default!;
    [Dependency] private readonly XenoEggRoleSystem _parasiteRole = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly XenoParasiteThrowerSystem _throwerSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FindParasiteComponent, FindParasiteActionEvent>(FindParasites);

        SubscribeLocalEvent<FindParasiteComponent, BoundUIOpenedEvent>(GetAllActiveParasiteSpawners);
        SubscribeLocalEvent<FindParasiteComponent, FollowParasiteSpawnerMessage>(FollowParasiteSpawner);
        SubscribeLocalEvent<FindParasiteComponent, TakeParasiteRoleMessage>(TakeParasiteRole);
    }

    private void FindParasites(Entity<FindParasiteComponent> parasiteFinderEnt, ref FindParasiteActionEvent args)
    {
        if (args.Handled || !_parasiteRole.UserCheck(parasiteFinderEnt.Owner))
        {
            return;
        }

        _ui.OpenUi(parasiteFinderEnt.Owner, XenoFindParasiteUI.Key, parasiteFinderEnt);
        args.Handled = true;
    }

    private void GetAllActiveParasiteSpawners(Entity<FindParasiteComponent> parasiteFinderEnt, ref BoundUIOpenedEvent args)
    {
        var ent = parasiteFinderEnt.Owner;
        var uiState = new FindParasiteUIState();

        var eggs = EntityQueryEnumerator<XenoEggComponent>();
        var eggMorphers = EntityQueryEnumerator<EggMorpherComponent>();
        var parasiteThrowers = EntityQueryEnumerator<XenoParasiteThrowerComponent>();
        var parasites = EntityQueryEnumerator<ParasiteAIComponent>();

        var spawners = new List<NetEntity>();
        while (eggs.MoveNext(out var eggEnt, out var egg))
        {
            if (egg.State != XenoEggState.Grown || (TryComp<XenoFragileEggComponent>(eggEnt, out var fragile) && fragile.SustainedBy != null))
            {
                continue;
            }

            var netEnt = _entities.GetNetEntity(eggEnt);
            spawners.Add(netEnt);
        }

        while (eggMorphers.MoveNext(out var eggMorpherEnt, out var eggMorpherComp))
        {
            if (eggMorpherComp.CurParasites < eggMorpherComp.ReservedParasites ||
                eggMorpherComp.CurParasites <= 0)
            {
                continue;
            }
            spawners.Add(_entities.GetNetEntity(eggMorpherEnt));
        }

        while (parasiteThrowers.MoveNext(out var throwerEnt, out var parasiteThrower))
        {
            var availableRegularParasites = parasiteThrower.CurParasites - parasiteThrower.ReservedParasites;
            var availableRoyalParasites = parasiteThrower.CurRoyalParasites - parasiteThrower.ReservedRoyalParasites;
            var totalAvailable = availableRegularParasites + availableRoyalParasites;

            if (totalAvailable <= 0 || _mob.IsDead(throwerEnt))
            {
                continue;
            }

            spawners.Add(_entities.GetNetEntity(throwerEnt));
        }

        while (parasites.MoveNext(out var paraEnt, out var parasite))
        {
            if (!_mob.IsAlive(paraEnt))
            {
                continue;
            }
            spawners.Add(_entities.GetNetEntity(paraEnt));
        }

        foreach (var spawner in spawners)
        {
            var spawnerEnt = _entities.GetEntity(spawner);
            var baseName = MetaData(spawnerEnt).EntityName;
            var areaName = Loc.GetString("xeno-ui-default-area-name");
            if (_areas.TryGetArea(spawnerEnt.ToCoordinates(), out var area, out _))
                areaName = Name(area.Value);

            if (TryComp<XenoParasiteThrowerComponent>(spawnerEnt, out var carrier))
            {
                var availableRegular = carrier.CurParasites - carrier.ReservedParasites;
                var availableRoyal = carrier.CurRoyalParasites - carrier.ReservedRoyalParasites;

                for (int i = 0; i < availableRegular; i++)
                {
                    var parasiteName = $"{baseName} ({Loc.GetString("xeno-ui-find-parasite-regular")})";
                    var name = Loc.GetString("xeno-ui-find-parasite-item",
                            ("itemName", parasiteName), ("areaName", areaName));
                    uiState.ActiveParasiteSpawners.Add(new(name, spawner, isRoyalParasite: false));
                }

                for (int i = 0; i < availableRoyal; i++)
                {
                    var parasiteName = $"{baseName} ({Loc.GetString("xeno-ui-find-parasite-royal")})";
                    var name = Loc.GetString("xeno-ui-find-parasite-item",
                            ("itemName", parasiteName), ("areaName", areaName));
                    uiState.ActiveParasiteSpawners.Add(new(name, spawner, isRoyalParasite: true));
                }
            }
            else
            {
                var name = Loc.GetString("xeno-ui-find-parasite-item",
                        ("itemName", baseName), ("areaName", areaName));
                uiState.ActiveParasiteSpawners.Add(new(name, spawner, isRoyalParasite: false));
            }
        }
        _ui.SetUiState(ent, XenoFindParasiteUI.Key, uiState);
    }

    private void FollowParasiteSpawner(Entity<FindParasiteComponent> parasiteFinderEnt, ref FollowParasiteSpawnerMessage args)
    {
        var netEnt = args.Entity;
        var ent = _entities.GetEntity(netEnt);

        var netSpawner = args.Spawner;
        var spawner = _entities.GetEntity(netSpawner);

        var ev = new GetVerbsEvent<AlternativeVerb>(ent, spawner, null, null, true, true, true, new());
        RaiseLocalEvent(ev);

        foreach (var action in ev.Verbs)
        {
            if (action.Text != Loc.GetString("verb-follow-text") || action.Act is null)
            {
                continue;
            }
            action.Act.Invoke();
            break;
        }
    }

    private void TakeParasiteRole(Entity<FindParasiteComponent> parasiteFinderEnt, ref TakeParasiteRoleMessage args)
    {
        var ghostEntity = parasiteFinderEnt.Owner;
        var netSpawner = args.Spawner;
        var spawner = _entities.GetEntity(netSpawner);

        if (TryComp<XenoParasiteThrowerComponent>(spawner, out var carrier))
        {
            var isRoyal = args.IsRoyalParasite;
            var msg = new CCMGhostTakeCarrierParasiteEvent(_entities.GetNetEntity(spawner), isRoyal);
            RaiseNetworkEvent(msg);
        }
        else if (TryComp<XenoParasiteComponent>(spawner, out _))
        {
            var isRoyal = TryComp<CCMRoyalParasiteComponent>(spawner, out _);
            var msg = new CCMGhostTakeParasiteEvent(_entities.GetNetEntity(spawner), isRoyal);
            RaiseNetworkEvent(msg);
        }
        else
        {
            var ev = new GetVerbsEvent<ActivationVerb>(ghostEntity, spawner, null, null, false, false, false, new());
            RaiseLocalEvent(spawner, ev);

            foreach (var action in ev.Verbs)
            {
                if (action.Text != Loc.GetString("rmc-xeno-egg-ghost-verb") || action.Act is null)
                {
                    continue;
                }
                action.Act.Invoke();
                break;
            }
        }
    }
}
