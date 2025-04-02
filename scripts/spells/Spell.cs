using Godot;

public abstract partial class Spell : Node, ICastable
{
	public string SpellName {get; protected set;}
	public float SpellDamage {get; protected set;}
	public float SpellCooldown {get; protected set;}
	public float SpellRange {get; set;}
	public float ManaCost {get; protected set;}
	public (Element, Element) SpellElements {get; protected set;}
	public PackedScene SpellScene {get; protected set;}

	//Cast spell - start position, direction, caster
	public Spell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, (Element, Element) SpellElements, string SpellScenePath){
		this.SpellName = SpellName;
		this.SpellDamage = SpellDamage;
		this.SpellCooldown = SpellCooldown;
		this.ManaCost = ManaCost;
		this.SpellElements = SpellElements;
		SpellScene = GD.Load(SpellScenePath) as PackedScene;
	}

	public Spell(SpellResource resource, (Element, Element) SpellElements){
		this.SpellName = resource.SpellName;
		this.SpellDamage = resource.SpellDamage;
		this.SpellCooldown = resource.SpellCooldown;
		this.SpellRange = resource.SpellRange;
		this.ManaCost = resource.ManaCost;
		this.SpellElements = SpellElements;
		this.SpellScene = resource.SpellScene;
	}

    public abstract void StartCasting(Vector3 position, Vector3 direction, Node3D caster);
    public abstract void HoldCasting(double delta);
    public abstract void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta);
    public abstract void EndCasting(Vector3 position, Vector3 direction, Node3D caster);

}
