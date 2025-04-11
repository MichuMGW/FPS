using Godot;

public partial class MeleeSpell : Spell
{
    
    public MeleeSpell(MeleeSpellResource resource, (Element, Element) SpellElements) : base(resource, SpellElements){

    }
    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster){}
    public override void EndCasting(Vector3 position, Vector3 direction, Node3D caster){}
    public override void HoldCasting(double delta){}
    public override void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta){}

}