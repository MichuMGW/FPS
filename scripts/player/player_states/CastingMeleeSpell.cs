using Godot;

public partial class CastingMeleeSpell : State
{
    private SpellCastManager scm;
    private ArmsAnimationPlayer animationPlayer;
    private AnimationTree animTree;
    private AnimationNodeStateMachinePlayback animState;
    private int currentAttack = 1;
    private int maxAttacks = 3;

    public override void Enter()
    {
        scm = Owner.GetNode<SpellCastManager>("SpellCastManager");
        animationPlayer = (ArmsAnimationPlayer)scm.LeftHand.animationPlayer;
        GD.Print($"Animation player path: {animationPlayer.GetPath()}");
        // Subskrybuj tylko raz!
        animationPlayer.MeleeAnimationFinished += OnMeleeAttackFinished;
        

        animTree = scm.GetParent().GetNode<AnimationTree>("Head/Camera3D/Arms/AnimationTree");
        animState = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");

        animTree.Active = true;

        animationPlayer = animTree.Owner.GetNode<ArmsAnimationPlayer>("AnimationPlayer");
        

        currentAttack = 1;
        PlayAttackAnimation(currentAttack);
    }

    private void PlayAttackAnimation(int attackNumber)
    {
        string animName = $"LMeleeAttack{attackNumber}";
        GD.Print($"Playing: {animName}");
        animState.Travel(animName);
    }

    private void OnMeleeAttackFinished(StringName animName)
    {
        GD.Print($"Received finished signal for {animName}");

        if (currentAttack < maxAttacks && Input.IsActionPressed("CastLeftSpell"))
        {
            currentAttack++;
            PlayAttackAnimation(currentAttack);
        }
        // else if (currentAttack == maxAttacks && Input.IsActionPressed("CastLeftSpell")){
        //     currentAttack = 1;
        //     PlayAttackAnimation(currentAttack);
        // }
        else
        {
            GD.Print("Attack sequence done");
            animState.Travel("LIdleMelee");

            // UWAGA: Przejście do Idle wymaga chwili — najlepiej dodać wait lub trigger po zakończeniu LIdle
            // Tu uproszczone: wychodzimy od razu
            EmitSignal(nameof(Transitioned), this, "PlayerIdle");
        }
    }

    public override void Exit()
    {
        GD.Print("Exiting CastingMeleeSpell state");
        animationPlayer.MeleeAnimationFinished -= OnMeleeAttackFinished;
    }

    public override void Update(double delta) { }
    public override void Physics_Update(double delta) { }
}

