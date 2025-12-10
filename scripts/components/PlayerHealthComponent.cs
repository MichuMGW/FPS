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
			EmitSignal(nameof(CurrentHealthChanged));
            if (value <= 0)
            {
                _currentHealth = 0;
                Die();
            }
            else if (value > MaxHealth)
            {
                _currentHealth = MaxHealth;
            }
            else _currentHealth = value;
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
        _stats.MaxHealthChanged += OnMaxHealthChanged;

        MaxHealth = _stats.MaxHealth;
		CurrentHealth = MaxHealth;

		Hurtbox.Hit += OnHit;

		// //TEST
		// enemy = GetParent<Enemy>();
		// enemy.Status.FireStarted += EnemyOnFire;
		// enemy.Status.FireEnded += EnemyOffFire;
		// //TEST

		// FireDamageTimer = GetNode<Timer>("FireDamageTimer");
		// FireDamageTimer.Timeout += OnFireDamageTimeout;
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


    private void OnMaxHealthChanged(float value)
    {
        MaxHealth = value;
        CurrentHealth = Mathf.Min(CurrentHealth + value, MaxHealth);
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
