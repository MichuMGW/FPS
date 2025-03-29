using Godot;
using System;

public partial class StatusComponent : Node
{
	//Można tu dodać float duration do sygnału
	// [Signal] public delegate void ApplyStatusEffectEventHandler(Element firstElement, Element secondElement);
	[Signal] public delegate void OnFireEventHandler();
	[Signal] public delegate void OnSlowedEventHandler();
	[Signal] public delegate void OnStunnedEventHandler();
	[Signal] public delegate void OnFrozenEventHandler();
	//TODO: Jakbym chciał dodać animacje przy spaleniu
	//[Signal] public delegate void OnBurnedEventHandler();
	public bool isOnFire = false;
	public bool isSlowed = false;
	public bool isStunned = false;
	public bool isFrozen = false;
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ApplyStatusEffect(Element firstElement, Element secondElement){
		if (firstElement == Element.Fire || secondElement == Element.Fire){
			isOnFire = true;
			EmitSignal(nameof(OnFire));
			GD.Print("On Fire");
		}
		if (firstElement == Element.Water || secondElement == Element.Water){
			isFrozen = true;
			GD.Print("Frozen");
		}
		if (firstElement == Element.Earth || secondElement == Element.Earth){
			isSlowed = true;
			GD.Print("Slowed");
		}
		if (firstElement == Element.Air || secondElement == Element.Air){
			isStunned = true;
			GD.Print("Stunned");
		}
	}
}
