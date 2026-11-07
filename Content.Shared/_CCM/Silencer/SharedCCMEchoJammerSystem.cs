using Content.Shared.Actions;
using Content.Shared.Audio;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._CCM.Silencer;

public abstract class SharedCCMEchoJammerSystem : EntitySystem
{
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMEchoJammerComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<CCMEchoJammerComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<CCMEchoJammerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnGetItemActions(Entity<CCMEchoJammerComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags is not { } flags || (flags & SlotFlags.BACK) == 0)
            return;

        args.AddAction(ref ent.Comp.Action, ent.Comp.ActionId);
        Dirty(ent);
    }

    private void OnGotUnequipped(Entity<CCMEchoJammerComponent> ent, ref GotUnequippedEvent args)
    {
        SetEnabled(ent, false);
    }

    private void OnShutdown(Entity<CCMEchoJammerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Enabled)
            ent.Comp.Enabled = false;
    }

    public void SetEnabled(Entity<CCMEchoJammerComponent> ent, bool enabled)
    {
        if (ent.Comp.Enabled == enabled)
            return;

        ent.Comp.Enabled = enabled;
        Dirty(ent);
        Actions.SetToggled(ent.Comp.Action, enabled);

        _ambient.SetAmbience(ent, enabled);
    }
}

public sealed partial class CCMEchoJammerToggleActionEvent : InstantActionEvent;
