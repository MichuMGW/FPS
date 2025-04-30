using Godot;
using System;

public partial class ArmsAnimationPlayer : AnimationPlayer
{
	[Signal] public delegate void MeleeAnimationFinishedEventHandler(StringName animName);

	private void EmitMeleeAnimationFinished(StringName animName){
		GD.Print("Animation Player: Melee attack finished!");
		EmitSignal(nameof(MeleeAnimationFinished), animName);
	}

}
