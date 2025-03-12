using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal]
	public delegate void EntityDiedEventHandler();

	[Export]
	public int maxHealth = 100;
	public int currentHealth;
	public bool isDead = false;

	public override void _Ready()
	{
		currentHealth = maxHealth;
	}


	public void TakeDamage(int damage)
	{
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			EmitSignal(nameof(EntityDied));
			Owner.QueueFree();
		}
	}
}
