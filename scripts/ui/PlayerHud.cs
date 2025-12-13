using Godot;

public partial class PlayerHud : Control
{
    private Player _player;
    private ProgressBar _healthBar;
    private ProgressBar _manaBar;
    private ProgressBar _expBar;

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
    }

    private void RefreshHealthMaxUI()
    {
        if (_player?.Health == null || _healthBar == null) return;

        _healthBar.MaxValue = _player.Health.MaxHealth;
    }

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
        _healthBar = GetNodeOrNull<ProgressBar>("VBoxContainer/HealthBar");
        _manaBar = GetNodeOrNull<ProgressBar>("VBoxContainer/ManaBar");
        _expBar = GetNodeOrNull<ProgressBar>("VBoxContainer/ExpBar");

        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (_healthBar == null) GD.PrintErr("PlayerHud: HealthBar not found at VBoxContainer/HealthBar");
        if (_manaBar == null) GD.PrintErr("PlayerHud: ManaBar not found at VBoxContainer/ManaBar");
        if (_expBar == null) GD.PrintErr("PlayerHud: ExpBar not found at VBoxContainer/ExpBar");
    }
}
