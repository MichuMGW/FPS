using Godot;
using System;

public partial class CastingProjectileSpell : State
{
	private SpellCastManager scm;
    private AnimationPlayer animationPlayer;
    private AnimationTree animTree;
    private AnimationNodeStateMachinePlayback animState;
    public override void Enter()
    {
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");
        animationPlayer = scm.LeftHand.animationPlayer;
        // animationPlayer.Play("StartCastingFireball");
		GD.Print("Casting left spell");

        animTree = scm.GetParent().GetNode<AnimationTree>("Head/Camera3D/Arms/AnimationTree");
        animState = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        
        animTree.Active = true;
        animState.Travel("LRaise");
    }

    public override void Exit()
    {
		GD.Print("Exiting CastingProjectileSpell state");
        animState.Travel("LWhitdraw");
        // animationPlayer.PlayBackwards("StartCastingFireball");
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
                animState.Travel("LCastProjectile1");
                // animationPlayer.Play("CastFireball");
				leftHand.StartCasting();
				scm.StartCooldown(leftHand.spell);
			}
		} else {
			EmitSignal(nameof(Transitioned), this, "PlayerIdle");
		}
		
    }
}
