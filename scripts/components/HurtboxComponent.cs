using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
	
	[Export] public EnemyHealthComponent healthComponent;
	// Called when the node enters the scene tree for the first time.

	public void OnAreaEntered(Area3D area){
		if(area is HitboxComponent){
			HitboxComponent hitbox = (HitboxComponent)area;
			healthComponent.TakeDamage(hitbox.damage);
			GD.Print("Enemy took" + hitbox.damage + "damage");
		}
	}

	
}
