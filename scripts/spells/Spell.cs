using Godot;

public abstract partial class Spell : Node
{
	public string SpellName {get; protected set;}
	public float SpellDamage {get; protected set;}
	public float SpellCooldown {get; protected set;}
	public float ManaCost {get; protected set;}
	public PackedScene SpellScene {get; protected set;}

	//Cast spell - start position, direction, caster
	public Spell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, string SpellScenePath){
		this.SpellName = SpellName;
		this.SpellDamage = SpellDamage;
		this.SpellCooldown = SpellCooldown;
		this.ManaCost = ManaCost;
		SpellScene = GD.Load(SpellScenePath) as PackedScene;
	}
	public abstract void Cast(Vector3 position, Vector3 direction, Node3D caster);
}
