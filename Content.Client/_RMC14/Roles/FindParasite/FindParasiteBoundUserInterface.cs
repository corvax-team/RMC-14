using Content.Shared._RMC14.Roles.FindParasite;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._RMC14.Roles.FindParasite;

[UsedImplicitly]
public sealed partial class FindParasiteBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey)
{
    private ItemList.Item? _selectedItem;
    private SpawnerData? _selectedSpawnerData;
    private bool _impledDeselect;

    private ItemList? _spawnerList;
    private List<SpawnerData>? _currentSpawners;

    [ViewVariables]
    private FindParasiteWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<FindParasiteWindow>();

        _spawnerList = _window.ParasiteSpawners;
        var spawnButton = _window.SpawnButton;

        _spawnerList.OnItemSelected += OnItemSelect;

        _spawnerList.OnItemDeselected += OnItemDeselect;

        spawnButton.Text = Loc.GetString("xeno-ui-find-parasite-spawn-button");
        spawnButton.Disabled = true;

        spawnButton.OnButtonDown += args =>
        {
            if (_selectedItem is null || _selectedSpawnerData is null)
            {
                args.Button.Disabled = true;
                return;
            }

            var selected = (NetEntity)_selectedItem.Metadata!;
            TakeParasiteRole(selected, _selectedSpawnerData.IsRoyalParasite);
            Close();
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FindParasiteUIState uiState ||
            _spawnerList is null)
        {
            return;
        }

        var activeParasiteSpawners = uiState.ActiveParasiteSpawners;
        _currentSpawners = activeParasiteSpawners;

        _selectedItem = null;
        _selectedSpawnerData = null;
        _spawnerList.Clear();

        foreach (var spawnerData in activeParasiteSpawners)
        {
            var item = new ItemList.Item(_spawnerList)
            {
                Text = spawnerData.Name,
                Metadata = spawnerData.Spawner,
            };
            _spawnerList.Add(item);
        }
    }

    private void OnItemSelect(ItemList.ItemListSelectedEventArgs args)
    {
        _window!.SpawnButton.Disabled = false;

        var newSelectedItem = args.ItemList[args.ItemIndex];
        var newSelected = (NetEntity)newSelectedItem.Metadata!;
        var newSelectedData = _currentSpawners?[args.ItemIndex];

        if (_selectedItem is null)
        {
            FollowParasiteSpawner(newSelected);
            _selectedItem = newSelectedItem;
            _selectedSpawnerData = newSelectedData;
            return;
        }

        if (_selectedItem != newSelectedItem)
        {
            _impledDeselect = true;
            _selectedItem.Selected = false;
            _impledDeselect = false;
        }

        _selectedItem = newSelectedItem;
        _selectedSpawnerData = newSelectedData;
        FollowParasiteSpawner(newSelected);
    }

    private void OnItemDeselect(ItemList.ItemListDeselectedEventArgs args)
    {
        if (_impledDeselect)
        {
            _impledDeselect = false;
            return;
        }
    }

    public void FollowParasiteSpawner(NetEntity spawner)
    {
        SendMessage(new FollowParasiteSpawnerMessage(spawner));
    }

    public void TakeParasiteRole(NetEntity spawner, bool isRoyalParasite = false)
    {
        SendMessage(new TakeParasiteRoleMessage(new NetEntity(owner.Id), spawner, isRoyalParasite));
    }
}
