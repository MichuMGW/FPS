using Godot;
using System;

public partial class HitboxComponent : Area3D
{
	[Export]
	public int damage = 10;
	//CHWILOWE
	// [Export]
	// public AnimationPlayer animationPlayer;

    // public override void _Ready()
    // {
    //     Monitorable = false;
    // }
    // CHWILOWE
    // public override void _Input(InputEvent @event)
    // {
    //     if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
	// 	{
	// 		//GD.Print("HitboxComponent clicked");
	// 		animationPlayer.Play("Attack");
	// 	}
    // }
}
