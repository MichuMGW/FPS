using Godot;
using System;

public partial class ProjectileSpell : Spell
{

	public PackedScene ProjectileScene;
	public ProjectileSpell()
	{
		SpellName = "Fireball";
		SpellCooldown = 2.0f;
		ManaCost = 20.0f;

		ProjectileScene = GD.Load("res://scenes/projectile.tscn") as PackedScene;
	}
    public override void Cast(Vector3 position, Vector3 direction, Node3D caster)
    {
        //Pocisk zostaje strzworzony na pozycji position, porusza się w kierunku direction, a casterem jest obiekt rzucający zaklęcie
		if (ProjectileScene == null){
			GD.PrintErr("ProjectileScene not found");
			return;
		}

		var projectileInstance = (Projectile)ProjectileScene.Instantiate();
		caster.GetParent().AddChild(projectileInstance);
		projectileInstance.GlobalTransform = caster.GlobalTransform;
		projectileInstance.Velocity = direction * projectileInstance.Speed;
    }

}
