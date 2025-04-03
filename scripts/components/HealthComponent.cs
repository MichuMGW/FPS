using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal] public delegate void EntityDiedEventHandler();

	[Export] public float maxHealth = 100f;
	private PackedScene floatingDamageScene;
	public float currentHealth;
	public bool isDead = false;
	public Enemy enemy;
	public Timer FireDamageTimer;

	public override void _Ready()
	{
		currentHealth = maxHealth;
		floatingDamageScene = GD.Load<PackedScene>("res://scenes/effects/floating_damage.tscn");

		//TEST
		enemy = GetParent<Enemy>();
		enemy.Status.OnFire += EnemyOnFire;
		//TEST
	}

	public void EnemyOnFire(bool isOnFire){
		if (isOnFire){
			GD.Print("ON FIRE");
			Timer FireDamageTimer = new Timer();
			FireDamageTimer.Name = "FireDamageTimer";
			FireDamageTimer.WaitTime = 0.5f; //Zmienić na pobieranie wartości od ziomka
			FireDamageTimer.OneShot = false;
			FireDamageTimer.Autostart = true;
			FireDamageTimer.Timeout += () => {
				TakeDamage(10);
				ShowDamage(enemy.GlobalPosition, 10);
			};
			AddChild(FireDamageTimer);
		} else {
			GD.Print("OFF FIRE");
			GetNode<Timer>("FireDamageTimer").QueueFree();
		}
	}

	public void TakeDamage(float damage)
	{
		currentHealth -= damage;
		currentHealth = Mathf.Max(0, currentHealth);
		if (currentHealth == 0)
		{
			EmitSignal(nameof(EntityDied));
			Owner.QueueFree();
		}
	}

	public void Heal(float amount){
		currentHealth += amount;
		currentHealth = Mathf.Min(maxHealth, currentHealth);
	}

	public void ShowDamage(Vector3 position, float damage){
		var floatingDamage = (FloatingDamage)floatingDamageScene.Instantiate();
		GetTree().CurrentScene.AddChild(floatingDamage);
		floatingDamage.SetDamage(damage);
		floatingDamage.GlobalPosition = position;
		floatingDamage.ShowDamage(damage);
	}
}
