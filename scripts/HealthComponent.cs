using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal] public delegate void EntityDiedEventHandler();

	[Export] public float maxHealth = 100f;
	private PackedScene floatingDamageScene;
	public float currentHealth;
	public bool isDead = false;

	public override void _Ready()
	{
		currentHealth = maxHealth;
		floatingDamageScene = GD.Load<PackedScene>("res://scenes/effects/floating_damage.tscn");
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
		GD.Print(GetTree().CurrentScene.Name);
		floatingDamage.SetDamage(damage);
		floatingDamage.GlobalPosition = position;
		floatingDamage.ShowDamage(damage);
	}
}
