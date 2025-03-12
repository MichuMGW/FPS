using Godot;

public abstract partial class Spell : Node
{
	public string SpellName {get; protected set;}
	public float SpellCooldown {get; protected set;}
	public float ManaCost {get; protected set;}

	//Cast spell - start position, direction, caster
	public abstract void Cast(Vector3 position, Vector3 direction, Node3D caster);
}
