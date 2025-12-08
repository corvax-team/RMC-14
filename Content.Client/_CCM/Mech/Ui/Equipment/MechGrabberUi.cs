﻿using Content.Client.UserInterface.Fragments;
using Content.Shared._CCM.Mech;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.Mech.Ui.Equipment;

public sealed partial class MechGrabberUi : UIFragment
{
    private CCMMechGrabberUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        if (fragmentOwner == null)
            return;

        _fragment = new CCMMechGrabberUiFragment();

        _fragment.OnEjectAction += e =>
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            userInterface.SendMessage(new MechGrabberEjectMessage(entManager.GetNetEntity(fragmentOwner.Value), entManager.GetNetEntity(e)));
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MechGrabberUiState grabberState)
            return;

        _fragment?.UpdateContents(grabberState);
    }
}
