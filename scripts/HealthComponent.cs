using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal]
	public delegate void EntityDiedEventHandler();

	[Export]
	public float maxHealth = 100f;
	public float currentHealth;
	public bool isDead = false;

	public override void _Ready()
	{
		currentHealth = maxHealth;
	}


	public void TakeDamage(float damage)
	{
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			EmitSignal(nameof(EntityDied));
			Owner.QueueFree();
		}
	}
}
