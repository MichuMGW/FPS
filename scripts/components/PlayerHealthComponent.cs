using Godot;
using System;

public partial class PlayerHealthComponent : Node
{
	[Signal] public delegate void EntityDiedEventHandler();
	[Signal] public delegate void CurrentHealthChangedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler();
	[Export] public HurtboxComponent Hurtbox {get; private set;}
	[Export] public PlayerStatsManager Stats {get; private set;}
	public float _maxHealth;
	public float MaxHealth {
		get
		{
			return _maxHealth;
		}
		set
		{
			_maxHealth = Mathf.Max(1f, value);
		}
	}
	private float _currentHealth;
	private float _healthRegenPerSecond;
	public float HealthRegenPerSecond
	{
		get
		{
			return _healthRegenPerSecond;
		}
		set
		{
			_healthRegenPerSecond = Mathf.Max(0f, value);
		}
	}
	public float CurrentHealth {
        get
        {
            return _currentHealth;
        }
        set
        {
			float newValue = Mathf.Clamp(value, 0f, MaxHealth);
			_currentHealth = newValue;

			EmitSignal(nameof(CurrentHealthChanged));

            if (_currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        GD.Print("PLAYER DIED");
		//DEBUG
		CurrentHealth = MaxHealth;
    }

	public override void _Ready()
	{

        Stats = GetParent().GetNode<PlayerStatsManager>("PlayerStatsManager");

        MaxHealth = Stats.GetStat(StatId.MaxHealth);
		CurrentHealth = MaxHealth;

		HealthRegenPerSecond = Stats.GetStat(StatId.HealthRegen);

		Stats.StatChanged += OnStatChanged;

		Hurtbox.Hit += OnHit;

		SetProcess(_healthRegenPerSecond > 0f);

		// //TEST
		// enemy = GetParent<Enemy>();
		// enemy.Status.FireStarted += EnemyOnFire;
		// enemy.Status.FireEnded += EnemyOffFire;
		// //TEST

		// FireDamageTimer = GetNode<Timer>("FireDamageTimer");
		// FireDamageTimer.Timeout += OnFireDamageTimeout;
	}

	public override void _Process(double delta)
    {
        if (CurrentHealth <= 0f || CurrentHealth >= MaxHealth)
            return;

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
            _healthRegenPerSecond = Mathf.Max(0f, newValue);
            SetProcess(_healthRegenPerSecond > 0f);
            return;
        }
    }


    private void OnHit(HitInfo hitInfo)
    {
		if (hitInfo.Source is not IDamageSource damageSource)
            return;
        
        float damage = damageSource.GetDamage();

		//TUTAJ MOGĘ DODAĆ RESISTY
		//np. damage = ApplyResistance(damage, damageType);

		TakeDamage(damage);
    }

    // public void EnemyOnFire(bool OnFire){
    // 	if (OnFire && !isCurrentlyOnFire){
    // 		isCurrentlyOnFire = true;
    // 		FireDamageTimer.Start();
    // 		GD.Print("ON FIRE");

    // 	} else if (!OnFire) {
    // 		isCurrentlyOnFire = false;
    // 		FireDamageTimer.Stop();
    // 		GD.Print("OFF FIRE");

    // 	}
    // }


    private void OnMaxHealthChanged(float newMax, float oldMax)
    {
        float previousMax = MaxHealth;
        MaxHealth = Mathf.Max(1f, newMax);

		float diff = MaxHealth - previousMax;
        if (diff > 0)
		{
			CurrentHealth = Mathf.Min(CurrentHealth + diff, MaxHealth);
		}
		else
		{
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
		}
	}

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
	}

	public void Heal(float amount)
	{
		if (amount <= 0f) return;

		CurrentHealth += amount;
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth);
	}
}
