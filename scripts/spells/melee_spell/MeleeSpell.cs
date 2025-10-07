using Godot;

public partial class MeleeSpell : Spell
{
    public float MeleeAttackCount { get; set; }
    public float CurrentAttack { get; set; }
    public MeleeSpell(MeleeSpellResource resource, (Element, Element) SpellElements) : base(resource, SpellElements)
    {
        MeleeAttackCount = resource.MeleeAttackCount;
        CurrentAttack = 0;
    }

    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        
    }
    public override void EndCasting(Vector3 position, Vector3 direction, Node3D caster){}
    public override void HoldCasting(double delta){}
    public override void HoldCasting(Vector3 position, Vector3 direction, Node3D caster, double delta){}

}