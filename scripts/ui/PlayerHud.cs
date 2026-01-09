using System;
using System.Collections.Generic;
using Godot;

public partial class PlayerHud : Control
{
    private static readonly Color InfoColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color ErrorColor = new Color(1f, 0.2f, 0.2f, 1f);

    private static readonly Color NormalTimeColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color BossTimeColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Export] public ElementKitDatabase KitDatabase;

    [Export] public NodePath HealthBarPath = "VBoxContainer/HealthBar";
    [Export] public NodePath ManaBarPath = "VBoxContainer/ManaBar";
    [Export] public NodePath ExpBarPath = "VBoxContainer/ExpBar";
    [Export] public NodePath ItemContainerPath = "ItemPanel/ItemFlowContainer";
    [Export] public NodePath HealthLabelPath = "VBoxContainer/HealthBar/HealthLabel";
    [Export] public NodePath ManaLabelPath = "VBoxContainer/ManaBar/ManaLabel";
    [Export] public NodePath ExpLabelPath = "VBoxContainer/ExpBar/ExpLabel";
    [Export] public NodePath GoldLabelPath = "GoldPanel/HBoxContainer/GoldLabel";
    [Export] public NodePath TimeLabelPath = "TimeLabel";
    [Export] public NodePath InfoLabelPath = "InfoLabel";

    [Export] public NodePath LeftSpellIconPath = "Spells/Left/SpellIcon";
    [Export] public NodePath RightSpellIconPath = "Spells/Right/SpellIcon";
    [Export] public NodePath DashSpellIconPath = "Spells/Dash/SpellIcon";

    [Export] public NodePath LeftSpellCooldownLabelPath = "Spells/Left/Cooldown";
    [Export] public NodePath RightSpellCooldownLabelPath = "Spells/Right/Cooldown";
    [Export] public NodePath DashSpellCooldownLabelPath = "Spells/Dash/Cooldown";

    [Export] public NodePath LeftSpellShortcutPath = "Spells/Left/Shortcut";
    [Export] public NodePath RightSpellShortcutPath = "Spells/Right/Shortcut";
    [Export] public NodePath DashSpellShortcutPath = "Spells/Dash/Shortcut";

    [Export] public NodePath BossHealthBarPath = "BossPanel/BossHealthBar";
    [Export] public NodePath BossNameLabelPath = "BossPanel/BossName";

    // Nazwy akcji z InputMap (USTAW w Inspectorze pod swoje actiony)
    [Export] public string LeftSpellAction = "cast_left";
    [Export] public string RightSpellAction = "cast_right";
    [Export] public string DashSpellAction = "cast_dash";

    private RunElementState _runElementState;

    private Player _player;
    private TextureProgressBar _healthBar;
    private Label _healthLabel;
    private TextureProgressBar _manaBar;
    private Label _manaLabel;
    private TextureProgressBar _expBar;
    private Label _expLabel;
    private Label _timeLabel;
    private FlowContainer _itemContainer;
    private ItemInventory _inventory;
    private ExperienceManager _experience;

    private TextureProgressBar _bossHealthBar;
    private Label _bossNameLabel;

    private Node _activeBoss;
    private BossHealthComponent _activeBossHealth;

    private Label _infoLabel;
    private Tween _infoTween;

    private Label _leftSpellShortcut, _rightSpellShortcut, _DashShortcut;
    private Label _leftCdLabel, _rightCdLabel, _dashCdLabel;
    private TextureRect _leftSpellIcon, _rightSpellIcon, _DashIcon;
    private PlayerSpellController _spellController;

    private GoldManager _gold;
    private Label _goldLabel;
    private GameEvents _events;
    private readonly Dictionary<string, ItemStackWidget> _itemWidgets = new();

    private bool _errorActive = false;

    private PlayerManaComponent _mana;

    private HealthComponent _bossHealth;
    private Node _bossNode;
    private float _bossPollAcc = 0f;
    private const float BossPollInterval = 0.1f;
    private float _lastBossHp = -1f;
    private bool _bossTimerMode = false;


    public override void _Ready()
    {
        Visible = false;

        FindNodes();

        if (_leftCdLabel != null) _leftCdLabel.Visible = false;
        if (_rightCdLabel != null) _rightCdLabel.Visible = false;
        if (_dashCdLabel != null) _dashCdLabel.Visible = false;

        if (_infoLabel != null)
        {
            _infoLabel.Visible = false;
            _infoLabel.Modulate = new Color(1, 1, 1, 1); // full alpha
        }

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
            _player.Stats.StatChanged += OnStatChanged;

            RefreshHealthMaxUI();
        }

        if (_mana != null && _manaBar != null)
        {
            _mana.ManaChanged += OnManaChanged;
            RefreshManaUI();
            RefreshManaMaxUI();
        }

        if (_inventory != null)
        {
            _inventory.InventoryChanged += OnInventoryChanged;
            RefreshInventoryUI();
        }

        if (_experience != null)
        {
            _experience.ExpChanged += (currentXp, currentLevel, xpToNext) =>
            {
                RefreshExpUI();
                RefreshExpMaxUI();
            };

            RefreshExpMaxUI();
            RefreshExpUI();
        }

        if (_gold != null)
        {
            _gold.GoldChanged += (newGold) =>
            {
                _goldLabel.Text = newGold.ToString();
            };

            _goldLabel.Text = _gold.CurrentGold.ToString();
            
            RefreshGoldUI();
        }

        if (_bossHealthBar != null)
            _bossHealthBar.Visible = false;
        if (_bossNameLabel != null)
            _bossNameLabel.Visible = false;

        if (_events != null)
        {
            _events.GameStarted += OnGameStarted;
            _events.RunTimeUpdated += RefreshTimeUI;
            _events.ElementPicked += OnElementPicked;

            _events.ShowInfo += OnShowInfo;
            _events.HideInfo += OnHideInfo;
            _events.ShowError += OnShowError;

            _events.BossSpawned += OnBossSpawned;
            _events.BossEnded += OnBossEnded;

            _events.BossFightStarted += OnBossFightStarted;
            _events.BossFightEnded += OnBossFightEnded;

            // ustaw skróty od razu (nie zależą od kitu)
            RefreshSpellShortcuts();
        }
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        UpdateSpellCooldownUI();
    }

    private void UpdateSpellCooldownUI()
    {
        if (_spellController == null)
            return;

        UpdateSlotCooldown(SpellSlot.LeftHand, _leftSpellIcon, _leftCdLabel);
        UpdateSlotCooldown(SpellSlot.RightHand, _rightSpellIcon, _rightCdLabel);
        UpdateSlotCooldown(SpellSlot.Dash, _DashIcon, _dashCdLabel);
    }

    private void UpdateSlotCooldown(SpellSlot slot, TextureRect icon, Label cdLabel)
    {
        var inst = _spellController.GetInstance(slot);

        if (inst == null || inst.Definition == null)
        {
            if (cdLabel != null) cdLabel.Visible = false;
            if (icon != null) icon.Modulate = new Color(1, 1, 1, 1);
            return;
        }

        float cd = inst.CurrentCooldown;

        if (cd > 0.01f)
        {
            // Tekst: np. 2.4s albo 3s
            // Ja wolę 1 miejsce po przecinku, bo "2.9" daje feeling responsywności.
            if (cdLabel != null)
            {
                cdLabel.Visible = true;
                cdLabel.Text = cd >= 10f ? $"{Mathf.CeilToInt(cd)}" : $"{cd:0.0}";
            }

            // Przyciemnij ikonę na cooldownie (czytelne i tanie)
            if (icon != null) icon.Modulate = new Color(1, 1, 1, 0.45f);
        }
        else
        {
            if (cdLabel != null) cdLabel.Visible = false;
            if (icon != null) icon.Modulate = new Color(1, 1, 1, 1);
        }
    }

    private void RefreshTimeUI(float elapsed, float total)
    {
        if (_timeLabel == null) return;

        if (_bossTimerMode)
        {
            // count up: elapsed = czas walki z bossem
            var minutes = Mathf.FloorToInt(elapsed / 60f);
            var seconds = Mathf.FloorToInt(elapsed % 60f);
            _timeLabel.Text = $"{minutes:00}:{seconds:00}";
            return;
        }

        // normal: countdown
        var remaining = total - elapsed;
        if (remaining < 0f) remaining = 0f;

        var m = Mathf.FloorToInt(remaining / 60f);
        var s = Mathf.FloorToInt(remaining % 60f);
        _timeLabel.Text = $"{m:00}:{s:00}";
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

        if (_mana != null)
            _mana.ManaChanged -= OnManaChanged;

        if (_inventory != null)
            _inventory.InventoryChanged -= OnInventoryChanged;

        if (_events != null)
        {
            _events.GameStarted -= OnGameStarted;
            _events.RunTimeUpdated -= RefreshTimeUI;
            _events.ElementPicked -= OnElementPicked;

            _events.ShowInfo -= OnShowInfo;
            _events.HideInfo -= OnHideInfo;
            _events.ShowError -= OnShowError;

            _events.BossSpawned -= OnBossSpawned;
            _events.BossEnded -= OnBossEnded;

            _events.BossFightStarted -= OnBossFightStarted;
            _events.BossFightEnded -= OnBossFightEnded;
        }
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

    private void OnGameStarted()
    {
        Visible = true;
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

        _healthBar.Value = _player.Health.CurrentHealth;
        RefreshHealthText();
    }

    private void RefreshHealthMaxUI()
    {
        if (_player?.Health == null || _healthBar == null) return;

        _healthBar.MaxValue = _player.Health.MaxHealth;
        RefreshHealthText();
    }

    private void OnManaChanged(float current, float max)
    {
        RefreshManaMaxUI();
        RefreshManaUI();
    }

    private void RefreshManaUI()
    {
        if (_mana == null || _manaBar == null) return;

        _manaBar.Value = _mana.CurrentMana;

        if (_manaLabel != null)
            _manaLabel.Text = $"{Mathf.FloorToInt(_mana.CurrentMana)} / {Mathf.FloorToInt(_mana.MaxMana)}";
    }

    private void RefreshManaMaxUI()
    {
        if (_mana == null || _manaBar == null) return;

        _manaBar.MaxValue = _mana.MaxMana;
    }

    private void RefreshExpUI()
    {
        if (_player?.Stats == null || _expBar == null) return;

        var currentExp = _experience.CurrentExp;
        _expBar.Value = currentExp;

        RefreshExpText();
    }

    private void RefreshExpMaxUI()
    {
        if (_experience == null || _expBar == null) return;

        var expToNext = _experience.ExpToNext;
        _expBar.MaxValue = expToNext;

        RefreshExpText();
    }

    private void RefreshExpText()
    {
        _expLabel.Text = $"{_experience.CurrentExp} / {_experience.ExpToNext}";
    }

    private void RefreshHealthText()
    {
        _healthLabel.Text = $"{Mathf.CeilToInt(_player.Health.CurrentHealth)} / {Mathf.CeilToInt(_player.Health.MaxHealth)}";
    }

    // private void RefreshManaText()
    // {
    //     _manaLabel.Text = $"{_player.Stats.GetStat(StatId.CurrentMana)} / {_player.Stats.GetStat(StatId.MaxMana)}";
    // }

    // Jeśli kiedyś podepniesz StatsChanged zamiast StatChanged
    private void RefreshGoldUI()
    {
        
    }
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

        _healthLabel = GetNodeOrNull<Label>(HealthLabelPath);
        _manaLabel = GetNodeOrNull<Label>(ManaLabelPath);
        _expLabel = GetNodeOrNull<Label>(ExpLabelPath);
        _goldLabel = GetNodeOrNull<Label>(GoldLabelPath);

        _leftSpellIcon = GetNodeOrNull<TextureRect>(LeftSpellIconPath);
        _rightSpellIcon = GetNodeOrNull<TextureRect>(RightSpellIconPath);
        _DashIcon = GetNodeOrNull<TextureRect>(DashSpellIconPath);

        _leftCdLabel = GetNodeOrNull<Label>(LeftSpellCooldownLabelPath);
        _rightCdLabel = GetNodeOrNull<Label>(RightSpellCooldownLabelPath);
        _dashCdLabel = GetNodeOrNull<Label>(DashSpellCooldownLabelPath);

        _leftSpellShortcut = GetNodeOrNull<Label>(LeftSpellShortcutPath);
        _rightSpellShortcut = GetNodeOrNull<Label>(RightSpellShortcutPath);
        _DashShortcut = GetNodeOrNull<Label>(DashSpellShortcutPath);

        _runElementState = GetTree().CurrentScene.GetNodeOrNull<RunElementState>("RunElementState");

        _itemContainer = GetNodeOrNull<FlowContainer>(ItemContainerPath);

        _inventory = GetTree().CurrentScene.GetNodeOrNull<ItemInventory>("ItemInventory");
        _experience = GetTree().CurrentScene.GetNodeOrNull<ExperienceManager>("ExperienceManager");
        _gold = GetTree().CurrentScene.GetNodeOrNull<GoldManager>("GoldManager");

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _timeLabel = GetNodeOrNull<Label>(TimeLabelPath);

        _infoLabel = GetNodeOrNull<Label>(InfoLabelPath);

        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        _spellController = _player?.Spells;
        if (_spellController == null && _player != null)
            _spellController = _player.GetNodeOrNull<PlayerSpellController>("PlayerSpellController");

        _mana = _player?.GetNodeOrNull<PlayerManaComponent>("PlayerManaComponent");

        _bossHealthBar = GetNodeOrNull<TextureProgressBar>(BossHealthBarPath);
        _bossNameLabel = GetNodeOrNull<Label>(BossNameLabelPath);

        if (_healthBar == null) GD.PrintErr("PlayerHud: HealthBar not found at VBoxContainer/HealthBar");
        if (_manaBar == null) GD.PrintErr("PlayerHud: ManaBar not found at VBoxContainer/ManaBar");
        if (_expBar == null) GD.PrintErr("PlayerHud: ExpBar not found at VBoxContainer/ExpBar");
    }

    private void OnElementPicked(int pickedElement, bool isSecondPick)
    {
        if (KitDatabase == null)
        {
            GD.PushWarning("[PlayerHud] KitDatabase is not assigned.");
            ClearSpellHud();
            return;
        }

        var picked = (Element)pickedElement;

        // Jaki element jest “aktywnym kitem”?
        // - pierwszy pick: po prostu element
        // - drugi pick: kombinacja first + picked
        Element kitElement = picked;

        if (isSecondPick && _runElementState != null)
        {
            var combined = ElementCombiner.Combine(_runElementState.First, picked);
            if (combined != Element.None)
                kitElement = combined;
        }

        ApplyKitToHud(kitElement);
    }

    private void ApplyKitToHud(Element kitElement)
    {
        var kit = KitDatabase.GetKit(kitElement);
        if (kit == null)
        {
            GD.PushWarning($"[PlayerHud] No kit found for element: {kitElement}");
            ClearSpellHud();
            return;
        }

        ApplySpellIcon(_leftSpellIcon, kit.LeftHandSpell);
        ApplySpellIcon(_rightSpellIcon, kit.RightHandSpell);
        ApplySpellIcon(_DashIcon, kit.DashSpell);

        // skróty możesz też odświeżać tutaj (koszt praktycznie zerowy)
        RefreshSpellShortcuts();
    }

    private void ApplySpellIcon(TextureRect iconRect, SpellDefinition def)
    {
        if (iconRect == null) return;
        iconRect.Texture = def?.Icon;
    }

    private void ClearSpellHud()
    {
        ApplySpellIcon(_leftSpellIcon, null);
        ApplySpellIcon(_rightSpellIcon, null);
        ApplySpellIcon(_DashIcon, null);

        if (_leftSpellShortcut != null) _leftSpellShortcut.Text = "";
        if (_rightSpellShortcut != null) _rightSpellShortcut.Text = "";
        if (_DashShortcut != null) _DashShortcut.Text = "";
    }

    private void RefreshSpellShortcuts()
    {
        if (_leftSpellShortcut != null) _leftSpellShortcut.Text = GetActionHintCompact(LeftSpellAction);
        if (_rightSpellShortcut != null) _rightSpellShortcut.Text = GetActionHintCompact(RightSpellAction);
        if (_DashShortcut != null) _DashShortcut.Text = GetActionHintCompact(DashSpellAction);
    }

    private string GetActionHintCompact(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName)) return "";
        if (!InputMap.HasAction(actionName)) return "";

        var evs = InputMap.ActionGetEvents(actionName);
        if (evs == null || evs.Count == 0) return "";

        // bierz pierwszy binding (najczęściej “primary”)
        var e = evs[0];

        // Zróbmy to krótsze niż AsText(), bo HUD to nie Wikipedia.
        if (e is InputEventMouseButton mb)
        {
            return mb.ButtonIndex switch
            {
                MouseButton.Left => "LMB",
                MouseButton.Right => "RMB",
                MouseButton.Middle => "MMB",
                MouseButton.WheelUp => "Wheel Up",
                MouseButton.WheelDown => "Wheel Down",
                _ => mb.AsText()
            };
        }

        if (e is InputEventKey k)
        {
            // Godot 4: zwykle lepiej PhysicalKeycode (układ klawiatury mniej miesza)
            var key = k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode;
            return OS.GetKeycodeString(key);
        }

        // fallback (np. pad)
        return e.AsText();
    }

    private void KillInfoTween()
    {
        if (_infoTween != null && _infoTween.IsRunning())
            _infoTween.Kill();
        _infoTween = null;
    }

    private void OnShowInfo(string message, float durationSeconds)
    {
        if (_infoLabel == null) return;
        if (_errorActive) return;

        KillInfoTween();

        _infoLabel.Text = message ?? "";
        _infoLabel.Visible = true;

        _infoLabel.Modulate = InfoColor;

        if (durationSeconds > 0f)
        {
            _infoTween = CreateTween();
            _infoTween.SetEase(Tween.EaseType.InOut);
            _infoTween.SetTrans(Tween.TransitionType.Sine);

            _infoTween.TweenInterval(durationSeconds);
            _infoTween.TweenProperty(_infoLabel, "modulate:a", 0f, 0.8f);

            _infoTween.Finished += () =>
            {
                if (_infoLabel == null) return;
                _infoLabel.Visible = false;

                var cc = _infoLabel.Modulate;
                cc.A = 1f;
                _infoLabel.Modulate = cc;

                _infoTween = null;
            };
        }
    }

    private void OnHideInfo()
    {
        if (_infoLabel == null) return;

        if (_errorActive) return;

        KillInfoTween();
        _infoLabel.Visible = false;

        var c = _infoLabel.Modulate;
        c.A = 1f;
        _infoLabel.Modulate = c;
    }

    private void OnShowError(string message, float durationSeconds)
    {
        if (_infoLabel == null) return;

        KillInfoTween();

        _errorActive = true;

        _infoLabel.Text = message ?? "";
        _infoLabel.Visible = true;

        _infoLabel.Modulate = ErrorColor;

        var hold = Mathf.Max(durationSeconds, 0.1f);

        _infoTween = CreateTween();
        _infoTween.SetEase(Tween.EaseType.InOut);
        _infoTween.SetTrans(Tween.TransitionType.Sine);

        _infoTween.TweenInterval(hold);
        _infoTween.TweenProperty(_infoLabel, "modulate:a", 0f, 1.0f);

        _infoTween.Finished += () =>
        {
            if (_infoLabel == null) return;
            _infoLabel.Visible = false;

            var cc = _infoLabel.Modulate;
            cc.A = 1f;
            _infoLabel.Modulate = cc;

            _infoTween = null;
            _errorActive = false;
        };
    }

    private void OnBossSpawned(Node boss, BossHealthComponent bossHealth, string displayName)
    {
        if (_bossHealthBar == null) return;
        if (bossHealth == null) return;

        // jeśli coś już było, wyczyść
        UnbindBoss();

        _activeBoss = boss;
        _activeBossHealth = bossHealth;

        // UI ON
        _bossHealthBar.Visible = true;
        _bossHealthBar.MaxValue = bossHealth.MaxHealth;
        _bossHealthBar.Value = bossHealth.CurrentHealth;

        if (_bossNameLabel != null)
        {
            _bossNameLabel.Visible = true;
            _bossNameLabel.Text = string.IsNullOrWhiteSpace(displayName) ? bossHealth.BossDisplayName : displayName;
        }

        // Subskrypcja sygnałów komponentu
        bossHealth.BossHealthChanged += OnBossHealthChanged;
        bossHealth.BossDied += OnBossDied;
    }

    private void OnBossFightStarted()
    {
        _bossTimerMode = true;

        if (_timeLabel != null)
        {
            _timeLabel.Modulate = BossTimeColor;
            _timeLabel.Text = "00:00";
        }
    }

    private void OnBossFightEnded()
    {
        _bossTimerMode = false;

        if (_timeLabel != null)
            _timeLabel.Modulate = NormalTimeColor;
    }


    private void OnBossHealthChanged(float current, float max)
    {
        if (_bossHealthBar == null) return;
        _bossHealthBar.MaxValue = max;
        _bossHealthBar.Value = current;
    }

    private void OnBossDied()
    {
        // BossDied = HP spadło do 0, ale encounter może jeszcze grać animacje.
        // Nie chowam paska tutaj. Schowamy na BossEnded z Mecha.
    }

    private void OnBossEnded(Node boss)
    {
        // chowamy tylko jeśli to ten aktywny boss
        if (_activeBoss == null) return;
        if (!GodotObject.IsInstanceValid(_activeBoss)) { UnbindBoss(); return; }

        if (boss == _activeBoss)
            UnbindBoss();
    }

    private void UnbindBoss()
    {
        if (_activeBossHealth != null && GodotObject.IsInstanceValid(_activeBossHealth))
        {
            _activeBossHealth.BossHealthChanged -= OnBossHealthChanged;
            _activeBossHealth.BossDied -= OnBossDied;
        }

        _activeBoss = null;
        _activeBossHealth = null;

        if (_bossHealthBar != null)
            _bossHealthBar.Visible = false;

        if (_bossNameLabel != null)
            _bossNameLabel.Visible = false;
    }


}
