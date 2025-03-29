using Godot;
using System;

public partial class ProjectileSpell : Spell
{
	public float ProjectileSpeed {get; set;}
	public float ProjectileSpread {get; set;}
	public float ProjectileRange {get; set;}
	private float cooldownTimer;
	public ProjectileSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, (Element, Element) SpellElements, string SpellScenePath, float ProjectileSpeed)
	: base(SpellName,SpellDamage,SpellCooldown,ManaCost,SpellElements,SpellScenePath)
	{
		this.ProjectileSpeed = ProjectileSpeed; 
	}

	public ProjectileSpell(ProjectileSpellResource resource, (Element, Element) SpellElements) : base(resource, SpellElements)
	{
		this.ProjectileSpeed = resource.ProjectileSpeed;
		this.ProjectileSpread = resource.ProjectileSpread;
		this.ProjectileRange = resource.ProjectileRange;
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
		projectileInstance.Elements = SpellElements;
		projectileInstance.Damage = SpellDamage;
		projectileInstance.Range = ProjectileRange;
		projectileInstance.GlobalTransform = caster.GlobalTransform;
		projectileInstance.startPosition = caster.GlobalTransform.Origin;
		
		//Projectile Spread
		float spreadX = Mathf.DegToRad((float)GD.RandRange(-ProjectileSpread, ProjectileSpread));
        float spreadY = Mathf.DegToRad((float)GD.RandRange(-ProjectileSpread, ProjectileSpread));

        direction = direction.Rotated(Vector3.Up, spreadX);
        direction = direction.Rotated(Vector3.Right, spreadY);

        projectileInstance.Velocity = direction.Normalized() * ProjectileSpeed;

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
