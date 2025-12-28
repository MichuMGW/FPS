using System.Collections.Generic;
using Godot;

public partial class PlayerHud : Control
{
    [Export] public NodePath HealthBarPath = "VBoxContainer/HealthBar";
    [Export] public NodePath ManaBarPath = "VBoxContainer/ManaBar";
    [Export] public NodePath ExpBarPath = "VBoxContainer/ExpBar";
    [Export] public NodePath ItemContainerPath = "ItemPanel/ItemFlowContainer";
    [Export] public NodePath HealthTextPath = "VBoxContainer/HealthBar/HealthText";
    [Export] public NodePath ManaTextPath = "VBoxContainer/ManaBar/ManaText";
    [Export] public NodePath ExpTextPath = "VBoxContainer/ExpBar/ExpText";

    private Player _player;
    private TextureProgressBar _healthBar;
    private Label _healthText;
    private TextureProgressBar _manaBar;
    private Label _manaText;
    private TextureProgressBar _expBar;
    private Label _expText;
    private FlowContainer _itemContainer;
    private ItemInventory _inventory;
    private readonly Dictionary<string, ItemStackWidget> _itemWidgets = new();

    public override void _Ready()
    {
        FindNodes();

        if (_player == null)
        {
            GD.PrintErr("PlayerHud: player not found in group 'player'.");
            return;
        }

        if (_player.Health != null)
        {
            _player.Health.CurrentHealthChanged += OnCurrentHealthChanged;
            // Ustawiamy startowe wartości
            RefreshHealthUI();
        }

        if (_player.Stats != null)
        {
            // Najlepiej słuchać per-stat (wydajniej i czytelniej)
            _player.Stats.StatChanged += OnStatChanged;

            // Dodatkowo możesz zostawić ogólne StatsChanged jeśli chcesz „fallback”
            // _player.Stats.StatsChanged += OnStatsChanged;

            RefreshHealthMaxUI();
        }

        if (_inventory != null)
        {
            _inventory.InventoryChanged += OnInventoryChanged;
            RefreshInventoryUI();
        }
    }

    public override void _ExitTree()
    {
        // Odpinamy eventy - inaczej dostaniesz call do freed instance
        if (_player?.Health != null)
            _player.Health.CurrentHealthChanged -= OnCurrentHealthChanged;

        if (_player?.Stats != null)
        {
            _player.Stats.StatChanged -= OnStatChanged;
            // _player.Stats.StatsChanged -= OnStatsChanged;
        }

        if (_inventory != null)
            _inventory.InventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        if (_itemContainer == null || _inventory == null)
            return;

        // zaznaczamy co jest “żywe” w tym odświeżeniu
        var alive = new HashSet<string>();

        foreach (var (def, count) in _inventory.GetStacks())
        {
            if (def == null) continue;
            if (count <= 0) continue;

            alive.Add(def.Id);

            if (!_itemWidgets.TryGetValue(def.Id, out var widget) || !IsInstanceValid(widget))
            {
                widget = new ItemStackWidget();
                _itemWidgets[def.Id] = widget;
                _itemContainer.AddChild(widget);
            }

            widget.SetData(def.Icon, count);
            widget.TooltipText = $"{def.DisplayName}\n{def.Description}";
        }

        // usuń widgety, których już nie ma
        var toRemove = new List<string>();
        foreach (var kv in _itemWidgets)
        {
            if (!alive.Contains(kv.Key))
                toRemove.Add(kv.Key);
        }

        foreach (var id in toRemove)
        {
            if (_itemWidgets.TryGetValue(id, out var w) && IsInstanceValid(w))
                w.QueueFree();

            _itemWidgets.Remove(id);
        }
    }

    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        switch ((StatId)statId)
        {
            case StatId.MaxHealth:
                RefreshHealthMaxUI();
                // Opcjonalnie: jeśli trzymasz % hp, to CurrentHealthChanged i tak poleci,
                // ale RefreshHealthUI jest bezpieczny.
                RefreshHealthUI();
                break;

            // Jak dodasz manę:
            // case StatId.MaxMana:
            //     RefreshManaMaxUI();
            //     RefreshManaUI();
            //     break;

            // case StatId.ExpToNextLevel:
            //     RefreshExpMaxUI();
            //     break;
        }
    }

    private void OnCurrentHealthChanged()
    {
        RefreshHealthUI();
    }

    private void RefreshHealthUI()
    {
        if (_player?.Health == null || _healthBar == null) return;

        float currentHealth = _player.Health.CurrentHealth;
        _healthBar.Value = currentHealth;
        RefreshHealthText();
    }

    private void RefreshHealthMaxUI()
    {
        if (_player?.Health == null || _healthBar == null) return;

        _healthBar.MaxValue = _player.Health.MaxHealth;
        RefreshHealthText();
    }

    // private void RefreshManaUI()
    // {
    //     if (_player?.Stats == null || _manaBar == null) return;

    //     float currentMana = _player.Stats.GetStat(StatId.CurrentMana);
    //     _manaBar.Value = currentMana;
    // }

    // private void RefreshManaMaxUI()
    // {
    //     if (_player?.Stats == null || _manaBar == null) return;

    //     float maxMana = _player.Stats.GetStat(StatId.MaxMana);
    //     _manaBar.MaxValue = maxMana;
    // }

    // private void RefreshExpUI()
    // {
    //     if (_player?.Stats == null || _expBar == null) return;

    //     float currentExp = _player.Stats.GetStat(StatId.CurrentExp);
    //     _expBar.Value = currentExp;
    // }

    private void RefreshHealthText()
    {
        _healthText.Text = $"{_player.Health.CurrentHealth} / {_player.Health.MaxHealth}";
    }

    // private void RefreshManaText()
    // {
    //     _manaText.Text = $"{_player.Stats.GetStat(StatId.CurrentMana)} / {_player.Stats.GetStat(StatId.MaxMana)}";
    // }

    // Jeśli kiedyś podepniesz StatsChanged zamiast StatChanged
    private void OnStatsChanged()
    {
        // Zrób proste „pełne odświeżenie”
        RefreshHealthMaxUI();
        RefreshHealthUI();
        // RefreshManaMaxUI();
        // RefreshManaUI();
        // RefreshExpMaxUI();
        // RefreshExpUI();
    }

    private void FindNodes()
    {
        _healthBar = GetNodeOrNull<TextureProgressBar>(HealthBarPath);
        _manaBar = GetNodeOrNull<TextureProgressBar>(ManaBarPath);
        _expBar = GetNodeOrNull<TextureProgressBar>(ExpBarPath);

        _healthText = GetNodeOrNull<Label>(HealthTextPath);
        _manaText = GetNodeOrNull<Label>(ManaTextPath);
        _expText = GetNodeOrNull<Label>(ExpTextPath);

        _itemContainer = GetNodeOrNull<FlowContainer>(ItemContainerPath);

        _inventory = GetTree().Root.GetNodeOrNull<ItemInventory>("ItemInventory");

        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (_healthBar == null) GD.PrintErr("PlayerHud: HealthBar not found at VBoxContainer/HealthBar");
        if (_manaBar == null) GD.PrintErr("PlayerHud: ManaBar not found at VBoxContainer/ManaBar");
        if (_expBar == null) GD.PrintErr("PlayerHud: ExpBar not found at VBoxContainer/ExpBar");
    }
}
