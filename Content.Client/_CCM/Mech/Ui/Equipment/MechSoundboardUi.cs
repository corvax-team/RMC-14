using Content.Client.UserInterface.Fragments;
using Content.Shared._CCM.Mech;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.Mech.Ui.Equipment;

public sealed partial class MechSoundboardUi : UIFragment
{
    private CCMMechSoundboardUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        if (fragmentOwner == null)
            return;

        _fragment = new CCMMechSoundboardUiFragment();
        _fragment.OnPlayAction += sound =>
        {
            // TODO: IDK dog
            userInterface.SendMessage(new MechSoundboardPlayMessage(IoCManager.Resolve<IEntityManager>().GetNetEntity(fragmentOwner.Value), sound));
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MechSoundboardUiState soundboardState)
            return;

        _fragment?.UpdateContents(soundboardState);
    }
}
