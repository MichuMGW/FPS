using Godot;
using System;

//Komponent zarządzający stanem zdrowia bytu.
public partial class HealthComponent : Node
{
    [Signal] public delegate void EntityDiedEventHandler();

	[Export] public float MaxHealth {get; set;} = 100f;
    [Export] public HurtboxComponent Hurtbox {get; set;}
	private bool isCurrentlyOnFire = false;
	private PackedScene floatingDamageScene;
    private float _currentHealth;
	public float CurrentHealth {
        get
        {
            return _currentHealth;
        }
        set
        {
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
	public bool isDead = false;
	public Enemy enemy;
	public Timer FireDamageTimer;
	private float fireDamage = 0f;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		floatingDamageScene = GD.Load<PackedScene>("res://scenes/effects/floating_damage.tscn");

		//DEBUG
		if(Hurtbox is null)
        {
            GD.PrintErr("Hurtbox is null");
        }

		//TEST
		// enemy = GetParent<Enemy>();
		// enemy.Status.FireStarted += EnemyOnFire;
		// enemy.Status.FireEnded += EnemyOffFire;
		//TEST

        Hurtbox.Hit += OnHit;

		FireDamageTimer = GetNode<Timer>("FireDamageTimer");
		FireDamageTimer.Timeout += OnFireDamageTimeout;
	}

    public void OnHit(HitInfo hitInfo)
    {
        if (hitInfo.Source is not IDamageSource damageSource)
            return;
        
        float damage = damageSource.GetDamage();
		damage *= hitInfo.DamageMultiplier;

		Element damageType = damageSource.GetDamageType();

		//TUTAJ MOGĘ DODAĆ RESISTY
		//np. damage = ApplyResistance(damage, damageType);

		TakeDamage(damage);
		ShowDamage(hitInfo.HitPosition, damage, new Color(1,0,0)); //KOLOR MOŻNA UZALEŻNIĆ OD ELEMENTU
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
	private void OnFireDamageTimeout(){
		ShowDamage(enemy.GlobalPosition, 10, new Color(1, 0, 0));
		TakeDamage(fireDamage);
	}

	public void EnemyOnFire(float damage){
		if (!isCurrentlyOnFire){
			isCurrentlyOnFire = true;
			fireDamage = damage;
			//DODAĆ DURATION I MOŻLIWOŚĆ JEGO ZMIANY
			FireDamageTimer.Start();
		}
	}

	private void EnemyOffFire(){
		isCurrentlyOnFire = false;
		FireDamageTimer.Stop();
	}

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
	}

    public void Die()
    {
        EmitSignal(nameof(EntityDied));
		Owner.QueueFree();
    }

	public void Heal(float amount){
		CurrentHealth += amount;
	}

	public void ShowDamage(Vector3 position, float damage, Color color){
		var floatingDamage = (FloatingDamage)floatingDamageScene.Instantiate();
		GetTree().CurrentScene.AddChild(floatingDamage);
		floatingDamage.GlobalPosition = position;
		floatingDamage.ShowDamage(damage, color);
	}
}
