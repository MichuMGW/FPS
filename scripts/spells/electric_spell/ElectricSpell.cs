using Godot;
using System;

public partial class ElectricSpell : Spell
{
    public ElectricSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, Element SpellElement, string SpellScenePath) : base(SpellName, SpellDamage, SpellCooldown, ManaCost, SpellElement, SpellScenePath)
    {
		//DODAĆ SCENE
    }

    // Called when the node enters the scene tree for the first time.
    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        return;
    }

	public override void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta)
    {
        
    }

	public override void HoldCasting(double delta)
    {
        throw new NotImplementedException();
    }
	
    public override void EndCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        throw new NotImplementedException();
    }

}
