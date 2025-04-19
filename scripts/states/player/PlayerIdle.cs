using Godot;
using System;

public partial class PlayerIdle : State
{
    private SpellCastManager scm;
    private double test = 0;
    private AnimationTree animTree;
    private AnimationNodeStateMachinePlayback animState;
    public override void Enter()
    {
        //TODO: Animacja domyślna
		// scm = GetParent() as SpellCastManager;
        //TODO: Uruchomienie animacji idle
        GD.Print("Entering PlayerIdle state");
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");

        animTree = scm.GetParent().GetNode<AnimationTree>("Head/Camera3D/Arms/AnimationTree");
        animState = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        
        animTree.Active = true;
        animState.Travel("LIdle");
    }

    public override void Exit()
    {
        return;
    }

    public override void Update(double delta)
    {
        test += delta;
        if (test > 1){
            GD.Print("Test");
            test = 0;
        }
        if (Input.IsActionPressed("CastLeftSpell")){
            //Castowanie spella
            SpellHand leftHand = scm.LeftHand;
            //Tutaj dodać czekanie na koniec animacji i zmianę stanu po skończeniu
            //ZASTĄPIĆ SWITCHEM??????
            if (!scm.isOnCooldown(leftHand.spell)){
                switch(leftHand.spell){
                    case ProjectileSpell projectileSpell:
                        EmitSignal(nameof(Transitioned), this, "CastingProjectileSpell");
                        break;
                    //DODAĆ INNE SPELLE
                    // case BeamSpell beamSpell:
                    //     leftHand.StartCasting();
                    //     break;
                    case MeleeSpell meleeSpell:
                        EmitSignal(nameof(Transitioned), this, "CastingMeleeSpell");
                        break;

                }
                // if (leftHand.spell is ProjectileSpell){
                //     EmitSignal(SignalName.Transitioned, this, "CastingProjectileSpell");
                // } else {
                //TODO: DODAĆ INNE STANY DLA INNYCH SPELLI
            }
        }
    }
    public override void Physics_Update(double delta)
    {
        return;
    }

}
