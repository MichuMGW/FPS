using Godot;
using System;

public partial class PlayerHealthComponent : Node
{
	[Signal] public delegate void EntityDiedEventHandler();
	[Signal] public delegate void CurrentHealthChangedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler();
	[Export] public HurtboxComponent Hurtbox {get; set;}
	public float MaxHealth {get; set;}
	private float _currentHealth;
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

    private PlayerStatsManager _stats;

	public override void _Ready()
	{

        _stats = GetParent().GetNode<PlayerStatsManager>("PlayerStatsManager");

        MaxHealth = _stats.GetStat(StatId.MaxHealth);
		if (MaxHealth <= 0f)
		{
			MaxHealth = 1f;
		}
		CurrentHealth = MaxHealth;

		_stats.StatChanged += OnStatChanged;

		Hurtbox.Hit += OnHit;

		// //TEST
		// enemy = GetParent<Enemy>();
		// enemy.Status.FireStarted += EnemyOnFire;
		// enemy.Status.FireEnded += EnemyOffFire;
		// //TEST

		// FireDamageTimer = GetNode<Timer>("FireDamageTimer");
		// FireDamageTimer.Timeout += OnFireDamageTimeout;
	}

    public override void _ExitTree()
    {
         if (_stats != null)
            _stats.StatChanged -= OnStatChanged;

        if (Hurtbox != null)
            Hurtbox.Hit -= OnHit;
    }


    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        if ((StatId)statId != StatId.MaxHealth)
            return;

        OnMaxHealthChanged(newValue, oldValue);
    }


    private void OnHit(HitInfo hitInfo)
    {
		if (hitInfo.Source is not IDamageSource damageSource)
            return;
        
        float damage = damageSource.GetDamage();
		damage *= hitInfo.DamageMultiplier;

		Element damageType = damageSource.GetDamageType();

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
	}

	// private void OnFireDamageTimeout(){
	// 	TakeDamage(fireDamage);
	// 	ShowDamage(enemy.GlobalPosition, 10, new Color(1, 0, 0));
	// }

	// public void EnemyOnFire(float damage){
	// 	if (!isCurrentlyOnFire){
	// 		isCurrentlyOnFire = true;
	// 		fireDamage = damage;
	// 		//DODAĆ DURATION I MOŻLIWOŚĆ JEGO ZMIANY
	// 		FireDamageTimer.Start();
	// 	}
	// }

	// private void EnemyOffFire(){
	// 	isCurrentlyOnFire = false;
	// 	FireDamageTimer.Stop();
	// }

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
	}

	public void Heal(float amount){
		CurrentHealth += amount;
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth);
	}

	// public void ShowDamage(Vector3 position, float damage, Color color){
	// 	var floatingDamage = (FloatingDamage)floatingDamageScene.Instantiate();
	// 	GetTree().CurrentScene.AddChild(floatingDamage);
	// 	floatingDamage.GlobalPosition = position;
	// 	floatingDamage.ShowDamage(damage, color);
	// }
}
