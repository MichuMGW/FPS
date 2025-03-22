using Godot;
using System;

public partial class ProjectileSpell : Spell
{
	public float ProjectileSpeed {get; set;}
	private float cooldownTimer;
	public ProjectileSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, Element SpellElement, string SpellScenePath, float ProjectileSpeed)
	: base(SpellName,SpellDamage,SpellCooldown,ManaCost,SpellElement,SpellScenePath)
	{
		this.ProjectileSpeed = ProjectileSpeed; 
	}
    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        //Pocisk zostaje strzworzony na pozycji position, porusza się w kierunku direction, a casterem jest obiekt rzucający zaklęcie
		CastFireball(position, direction, caster);
		cooldownTimer = SpellCooldown; 
    }

	public override void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta)
    {
		cooldownTimer -= (float)delta;
        
		if (cooldownTimer <= 0f){
			CastFireball(position, direction, caster);
			cooldownTimer = SpellCooldown;
		}
    }

	public void CastFireball(Vector3 position, Vector3 direction, Node3D caster){
		if (SpellScene == null){
			GD.PrintErr("ProjectileScene not found");
			return;
		}
		var projectileInstance = (Projectile)SpellScene.Instantiate();
		caster.Owner.GetParent().AddChild(projectileInstance);
		projectileInstance.Speed = ProjectileSpeed;
		projectileInstance.GlobalTransform = caster.GlobalTransform;
		projectileInstance.Velocity = direction.Normalized() * projectileInstance.Speed;
	}

    public override void HoldCasting(double delta)
    {
        return;
    }
	    public override void EndCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        return;
    }

}
