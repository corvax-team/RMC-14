// using Content.Server.Mind;
// using Content.Shared._TGMC14.FPV;
// using Content.Server.FpvDrone;
// using Robust.Shared.GameObjects;

// namespace Content.Server._TGMC14.FPV;

// public sealed class FpvDroneEjectSystem : EntitySystem
// {
//     [Dependency] private readonly MindSystem _mind = default!;
//     [Dependency] private readonly IEntityManager _entityManager = default!;

//     public override void Initialize()
//     {
//         base.Initialize();
//         SubscribeLocalEvent<FpvDroneObserverComponent, DroneEjectEvent>(OnEject);
//     }

//     private void OnEject(EntityUid uid, FpvDroneObserverComponent component, DroneEjectEvent args)
//     {
//         if (!TryComp<FpvDroneControlComponent>(component.Control, out var control))
//             return;

//         if (!_mind.TryGetMind(uid, out var mindId, out var mind))
//             return;

//         if (control.Pilot == EntityUid.Invalid)
//             return;

//         // Начинаем с пилота, которого уже знаем
//         var targetPilot = control.Pilot;

//         // Если в событии пришел NetId, получаем EntityUid через IEntityManager
//         if (args.PilotNetId.HasValue)
//         {
//             var netId = args.PilotNetId.Value;
//             if (_entityManager.TryGetEntity(netId, out var pilotEntity))
//             {
//                 targetPilot = pilotEntity.Uid;
//             }
//         }

//         // Переводим контроль обратно к пилоту
//         _mind.TransferTo(mindId, targetPilot, mind: mind);

//         // Сбрасываем ссылки в дроне
//         control.Pilot = EntityUid.Invalid;
//         component.Control = EntityUid.Invalid;
//     }
// }