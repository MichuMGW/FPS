using Godot;
using System;

public partial class PlayerIdle : State
{
    private SpellCastManager scm;
    public override void Enter()
    {
        //TODO: Animacja domyślna
		// scm = GetParent() as SpellCastManager;
        //TODO: Uruchomienie animacji idle
        GD.Print("Entering PlayerIdle state");
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");
    }

    public override void Exit()
    {
        return;
    }

    public override void Update(double delta)
    {
        if (Input.IsActionPressed("CastLeftSpell")){
            //Castowanie spella
            SpellHand leftHand = scm.LeftHand;
            // leftHand.StartCasting();
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

                }
                // if (leftHand.spell is ProjectileSpell){
                //     EmitSignal(SignalName.Transitioned, this, "CastingProjectileSpell");
                // } else {
                //TODO: DODAĆ INNE STANY DLA INNYCH SPELLI
            }
        }
            
            // EmitSignal(SignalName.Transitioned, this, "CastingProjectileSpell");
    }
    public override void Physics_Update(double delta)
    {
        return;
    }

}
