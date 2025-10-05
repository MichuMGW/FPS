using Godot;
using System;

public partial class PlayerHealthComponent : Node
{
	[Signal] public delegate void EntityDiedEventHandler();

	private float _maxHealth;
	// private bool isCurrentlyOnFire = false;
	// private PackedScene floatingDamageScene;
	public float currentHealth;
	// public bool isDead = false;
	private PlayerStats _stats;
	// public Timer FireDamageTimer;
	// private float fireDamage = 0f;


	public override void _Ready()
	{

        _stats = GetNode<PlayerStats>("/root/PlayerStats");
        _stats.MaxHealthChanged += OnMaxHealthChanged;

        _maxHealth = _stats.MaxHealth;
		currentHealth = _maxHealth;
		floatingDamageScene = GD.Load<PackedScene>("res://scenes/effects/floating_damage.tscn");

		//TEST
		enemy = GetParent<Enemy>();
		enemy.Status.FireStarted += EnemyOnFire;
		enemy.Status.FireEnded += EnemyOffFire;
		//TEST

		FireDamageTimer = GetNode<Timer>("FireDamageTimer");
		FireDamageTimer.Timeout += OnFireDamageTimeout;
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
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth + value, maxHealth);
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
		currentHealth -= damage;
		currentHealth = Mathf.Max(0, currentHealth);
		if (currentHealth == 0)
		{
			EmitSignal(nameof(EntityDied));
			// Owner.QueueFree();
		}
	}

	public void Heal(float amount){
		currentHealth += amount;
		currentHealth = Mathf.Min(maxHealth, currentHealth);
	}

	// public void ShowDamage(Vector3 position, float damage, Color color){
	// 	var floatingDamage = (FloatingDamage)floatingDamageScene.Instantiate();
	// 	GetTree().CurrentScene.AddChild(floatingDamage);
	// 	floatingDamage.GlobalPosition = position;
	// 	floatingDamage.ShowDamage(damage, color);
	// }
}
