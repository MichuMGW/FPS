using Godot;
using System;

public partial class CastingProjectileSpell : State
{
	private SpellCastManager scm;
    public override void Enter()
    {
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");
		GD.Print("Casting left spell");
    }

    public override void Exit()
    {
		GD.Print("Exiting CastingProjectileSpell state");
		//Dodać animację
		//Tutaj dodać czekanie na koniec animacji i zmianę stanu po skończeniu
        return;
    }

    public override void Physics_Update(double delta)
    {
        return;
    }

    public override void Update(double delta)
    {
        if(Input.IsActionPressed("CastLeftSpell")){
			SpellHand leftHand = scm.LeftHand;
			if (!scm.isOnCooldown(leftHand.spell)){
				leftHand.StartCasting();
				scm.StartCooldown(leftHand.spell);
			}
		} else {
			EmitSignal(nameof(Transitioned), this, "PlayerIdle");
		}
		
    }
}
