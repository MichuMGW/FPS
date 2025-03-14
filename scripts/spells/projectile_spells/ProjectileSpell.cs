using Godot;
using System;

public partial class ProjectileSpell : Spell
{
	public float ProjectileSpeed {get; set;}
	public ProjectileSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, string SpellScenePath, float ProjectileSpeed)
	: base(SpellName,SpellDamage,SpellCooldown,ManaCost,SpellScenePath){
		this.ProjectileSpeed = ProjectileSpeed; 
	}
    public override void Cast(Vector3 position, Vector3 direction, Node3D caster)
    {
        //Pocisk zostaje strzworzony na pozycji position, porusza się w kierunku direction, a casterem jest obiekt rzucający zaklęcie
		if (SpellScene == null){
			GD.PrintErr("ProjectileScene not found");
			return;
		}

		var projectileInstance = (Projectile)SpellScene.Instantiate();
		caster.GetParent().AddChild(projectileInstance);
		projectileInstance.Speed = ProjectileSpeed;
		projectileInstance.GlobalTransform = caster.GlobalTransform;
		projectileInstance.Velocity = direction * projectileInstance.Speed;
    }

}
