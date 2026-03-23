// using Robust.Client.Graphics;
// using Robust.Client.GameObjects;
// using Robust.Shared.GameObjects;
// using Robust.Shared.Prototypes;
// using Content.Shared._CM14.FpvDrone;
// using Content.Client.FpvDrone;

// namespace Content.Client._CM14.FpvDrone
// {
//     [RegisterComponent]
//     public sealed partial class FpvDroneClientComponent : SharedFpvDroneComponent
//     {
//         [Dependency] private readonly IOverlayManager _overlays = default!;
//         [Dependency] private readonly IPrototypeManager _protoMan = default!;

//         private FpvDroneOverlay? _overlay;

//         // Включение оверлея
//         public void EnableOverlay()
//         {
//             if (_overlay != null)
//                 return;

//             _overlay = new FpvDroneOverlay(_protoMan);
//             _overlays.AddOverlay(_overlay);
//             Enabled = true;
//         }

//         // Выключение оверлея
//         public void DisableOverlay()
//         {
//             if (_overlay == null)
//                 return;

//             _overlays.RemoveOverlay(_overlay);
//             _overlay = null;
//             Enabled = false;
//         }
//     }
// }