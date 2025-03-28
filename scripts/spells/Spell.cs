using Godot;

public abstract partial class Spell : Node3D, ICastable
{
	public string SpellName {get; protected set;}
	public float SpellDamage {get; protected set;}
	public float SpellCooldown {get; protected set;}
	public float ManaCost {get; protected set;}
	public Element SpellElement {get; protected set;}
	public PackedScene SpellScene {get; protected set;}

	//Cast spell - start position, direction, caster
	public Spell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, Element SpellElement, string SpellScenePath){
		this.SpellName = SpellName;
		this.SpellDamage = SpellDamage;
		this.SpellCooldown = SpellCooldown;
		this.ManaCost = ManaCost;
		this.SpellElement = SpellElement;
		SpellScene = GD.Load(SpellScenePath) as PackedScene;
	}

    public abstract void StartCasting(Vector3 position, Vector3 direction, Node3D caster);
    public abstract void HoldCasting(double delta);
    public abstract void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta);
    public abstract void EndCasting(Vector3 position, Vector3 direction, Node3D caster);

}
