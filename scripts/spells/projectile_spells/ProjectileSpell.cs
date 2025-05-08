using Godot;
using System;

public partial class ProjectileSpell : Spell
{
	public float ProjectileSpeed {get; set;}
	public float ProjectileSpread {get; set;}
	//Do usunięcia
	private float cooldownTimer;
	//Koniec do usunięcia
	public ProjectileSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, (Element, Element) SpellElements, string SpellScenePath, float ProjectileSpeed)
	: base(SpellName,SpellDamage,SpellCooldown,ManaCost,SpellElements,SpellScenePath)
	{
		this.ProjectileSpeed = ProjectileSpeed; 
	}

	public ProjectileSpell(ProjectileSpellResource resource, (Element, Element) SpellElements) : base(resource, SpellElements)
	{
		this.ProjectileSpeed = resource.ProjectileSpeed;
		this.ProjectileSpread = resource.ProjectileSpread;
	}

    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        if (SpellScene == null){
			GD.PrintErr("ProjectileScene not found");
			return;
		}
		var projectileInstance = (Projectile)SpellScene.Instantiate();
		caster.GetTree().CurrentScene.AddChild(projectileInstance);
		projectileInstance.Elements = SpellElements;
		projectileInstance.Damage = SpellDamage;
		projectileInstance.Range = SpellRange;
		projectileInstance.GlobalTransform = caster.GlobalTransform;
		projectileInstance.Scale = new Vector3(1, 1, 1);
		projectileInstance.startPosition = caster.GlobalTransform.Origin;
		
		//Projectile Spread
		float spreadX = Mathf.DegToRad((float)GD.RandRange(-ProjectileSpread, ProjectileSpread));
        float spreadY = Mathf.DegToRad((float)GD.RandRange(-ProjectileSpread, ProjectileSpread));

        direction = direction.Rotated(Vector3.Up, spreadX);
        direction = direction.Rotated(Vector3.Right, spreadY);

        projectileInstance.Velocity = direction.Normalized() * ProjectileSpeed;
    }

	public override void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta)
    {
		return;
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
