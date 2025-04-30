using Godot;
using System;

public partial class ArmsAnimationManager : Node
{
	[Signal] public delegate void MeleeAnimationFinishedEventHandler(StringName animName);
	
	private AnimationPlayer animPlayer;
	public override void _Ready()
	{
		animPlayer = Owner.GetNode<AnimationPlayer>("AnimationPlayer");
		// var animPlayer = GetNode<AnimationPlayer>("Path/To/AnimationPlayer");
		// var animTree = Owner.GetNode<AnimationTree>("AnimationTree");
		// animTreePlayback = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback"); 
		// // this.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
		animPlayer.AnimationFinished += OnAnimationFinished;
	}

	// private void OnAnimationFinished(StringName animName)
	// {
	// 	GD.Print("Skończyła się animacja: " + animName);
	// 	if (animName == "LMeleeAttack1")
	// 	{
	// 		if (Input.IsActionPressed("CastLeftSpell"))
	// 		{
	// 			animTreePlayback.Travel("LMeleeAttack2");
	// 		}
	// 		else
	// 		{
	// 			animTreePlayback.Travel("LIdle");
	// 		}
	// 	}
	// 	else if (animName == "LMeleeAttack2")
	// 	{
	// 		if (Input.IsActionPressed("CastLeftSpell"))
	// 		{
	// 			animTreePlayback.Travel("LMeleeAttack3");
	// 		}
	// 		else
	// 		{
	// 			animTreePlayback.Travel("LIdle");
	// 		}
	// 	}
	// 	else if (animName == "LMeleeAttack3")
	// 	{
	// 		animTreePlayback.Travel("LIdle");
	// 	}
	// }

	private void OnAnimationFinished(StringName animName){
		if (animName == "LMeleeAttack1" || animName == "LMeleeAttack2" || animName == "LMeleeAttack3"){
			EmitSignal(nameof(MeleeAnimationFinished), animName);
		}
	}

}
