using Content.Shared._CCM.Teleporter;
using Content.Shared._RMC14.Areas;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

namespace Content.Client._CCM.Teleporter.UI;

[UsedImplicitly]
public sealed class CCMPlanetTeleporterBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    private CCMPlanetTeleporterWindow? _window;

    public CCMPlanetTeleporterBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<CCMPlanetTeleporterWindow>();
        _window.OnSelected += pos => SendMessage(new CCMPlanetTeleporterSelectMsg(pos));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null || state is not CCMPlanetTeleporterState s)
            return;

        if (s.MapEntity == NetEntity.Invalid)
        {
            _window.SetStatus(Loc.GetString("ccm-planet-teleporter-no-map"));
            return;
        }

        var mapEntity = _entMan.GetEntity(s.MapEntity);
        if (!_entMan.TryGetComponent(mapEntity, out AreaGridComponent? areaGrid))
        {
            _window.SetStatus(Loc.GetString("ccm-planet-teleporter-no-map"));
            return;
        }

        _window.TacticalMap.SetCurrentMap(mapEntity);
        _window.TacticalMap.SetCurrentMapName(s.MapName);
        _window.TacticalMap.UpdateTexture((mapEntity, areaGrid));

        var status = s.CooldownRemaining > TimeSpan.Zero
            ? Loc.GetString("ccm-planet-teleporter-cooldown",
                ("time", $"{(int) Math.Ceiling(s.CooldownRemaining.TotalSeconds)}s"))
            : s.Teleported
                ? Loc.GetString("ccm-planet-teleporter-status-return")
                : Loc.GetString("ccm-planet-teleporter-status-teleport");

        _window.SetStatus(status);
    }
}
