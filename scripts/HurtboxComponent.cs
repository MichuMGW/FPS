using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
	
	[Export] public HealthComponent healthComponent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void OnAreaEntered(Area3D area){
		if(area is HitboxComponent){
			HitboxComponent hitbox = (HitboxComponent)area;
			healthComponent.TakeDamage(hitbox.damage);
			GD.Print("Enemy took" + hitbox.damage + "damage");
		}
	}

	
}
