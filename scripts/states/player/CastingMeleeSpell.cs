using Godot;

public partial class CastingMeleeSpell : State
{
    private SpellCastManager scm;
    private SpellHand leftHand;
    private MeleeSpell meleeSpell;
    private int currentAttack = 0;
    private int maxAttacks = 3;
    public override void Enter()
    {
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");
    }

    public override void Exit()
    {
        GD.Print("Exiting CastingMeleeSpell state");
        return;
    }

    public override void Physics_Update(double delta)
    {
        return;
    }

    public override void Update(double delta)
    {
        if(Input.IsActionPressed("CastLeftSpell")){
            leftHand = scm.LeftHand;
            meleeSpell = leftHand.spell as MeleeSpell;
            if (!scm.isOnCooldown(leftHand.spell)){

            }
        }
    }
}