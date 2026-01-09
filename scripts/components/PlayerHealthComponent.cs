using Godot;
using System;

public partial class PlayerHealthComponent : Node
{
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void CurrentHealthChangedEventHandler();
    [Signal] public delegate void HealthChangedEventHandler();

    [Export] public HurtboxComponent Hurtbox { get; private set; }
    [Export] public PlayerStatsManager Stats { get; private set; }

    private GameEvents _events;

    private bool _isDead = false;

    public float _maxHealth;
    public float MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = Mathf.Max(1f, value);
    }

    private float _currentHealth;

    private float _healthRegenPerSecond;
    public float HealthRegenPerSecond
    {
        get => _healthRegenPerSecond;
        set => _healthRegenPerSecond = Mathf.Max(0f, value);
    }

    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            if (_isDead) return;

            float newValue = Mathf.Clamp(value, 0f, MaxHealth);

            // nie spamuj sygnałem jak nic się nie zmieniło
            if (Mathf.IsEqualApprox(newValue, _currentHealth))
                return;

            _currentHealth = newValue;
            EmitSignal(nameof(CurrentHealthChanged));

            if (_currentHealth <= 0f)
                Die();
        }
    }

    public override void _Ready()
    {
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");

        Stats = GetParent().GetNodeOrNull<PlayerStatsManager>("PlayerStatsManager");
        if (Stats == null)
        {
            GD.PushError("[PlayerHealthComponent] Missing PlayerStatsManager.");
            return;
        }

        MaxHealth = Stats.GetStat(StatId.MaxHealth);
        _currentHealth = MaxHealth;
        EmitSignal(nameof(CurrentHealthChanged));

        HealthRegenPerSecond = Stats.GetStat(StatId.HealthRegen);

        Stats.StatChanged += OnStatChanged;

        if (Hurtbox != null)
            Hurtbox.Hit += OnHit;
        else
            GD.PushWarning("[PlayerHealthComponent] Hurtbox is null.");

        SetProcess(HealthRegenPerSecond > 0f);
    }

    public override void _Process(double delta)
    {
        if (_isDead) return;
        if (_currentHealth <= 0f || _currentHealth >= MaxHealth) return;

        float dt = (float)delta;
        Heal(HealthRegenPerSecond * dt);
    }

    public override void _ExitTree()
    {
        if (Stats != null)
            Stats.StatChanged -= OnStatChanged;

        if (Hurtbox != null)
            Hurtbox.Hit -= OnHit;
    }

    private void Die()
{
    if (_isDead) return;
    _isDead = true;

    SetProcess(false);

    if (Hurtbox != null)
        Hurtbox.Hit -= OnHit;

    _currentHealth = 0f;
    EmitSignal(nameof(CurrentHealthChanged));

    EmitSignal(nameof(PlayerDied));

    var events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
    events.CallDeferred(nameof(GameEvents.EndRun), (int)RunEndReason.PlayerDied);
    }


    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        var id = (StatId)statId;

        if (id == StatId.MaxHealth)
        {
            OnMaxHealthChanged(newValue, oldValue);
            return;
        }

        if (id == StatId.HealthRegen)
        {
            HealthRegenPerSecond = newValue;
            SetProcess(!_isDead && HealthRegenPerSecond > 0f);
            return;
        }
    }

    private void OnHit(HitInfo hitInfo)
    {
        if (_isDead) return;

        if (hitInfo.Source is not IDamageSource damageSource)
            return;

        float damage = damageSource.GetDamage();
        TakeDamage(damage);
    }

    private void OnMaxHealthChanged(float newMax, float oldMax)
    {
        float previousMax = MaxHealth;
        MaxHealth = Mathf.Max(1f, newMax);

        float diff = MaxHealth - previousMax;

        if (_isDead) return;

        if (diff > 0f)
            CurrentHealth = Mathf.Min(CurrentHealth + diff, MaxHealth);
        else
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        CurrentHealth -= damage;
    }

    public void Heal(float amount)
    {
        if (_isDead) return;
        if (amount <= 0f) return;
        CurrentHealth += amount;
    }
}
