using Godot;
using System;
using Godot.Collections;

public partial class StatusComponent : Node
{
	//Można tu dodać float duration do sygnału
	// [Signal] public delegate void ApplyStatusEffectEventHandler(Element firstElement, Element secondElement);
	[Signal] public delegate void OnFireEventHandler();
	[Signal] public delegate void OnSlowedEventHandler();
	[Signal] public delegate void OnStunnedEventHandler();
	[Signal] public delegate void OnFrozenEventHandler();
	[Signal] public delegate void OnFireTimeoutEventHandler();
	[Signal] public delegate void OnSlowedTimeoutEventHandler();
	[Signal] public delegate void OnStunnedTimeoutEventHandler();
	[Signal] public delegate void OnFrozenTimeoutEventHandler();
	//TODO: Jakbym chciał dodać animacje przy spaleniu
	//[Signal] public delegate void OnBurnedEventHandler();
	public bool IsOnFire {get; set;} = false;
	public bool IsSlowed {get; set;} = false;
	public bool IsStunned {get; set;} = false;
	public bool IsFrozen {get; set;} = false;
	public Timer FireTimer {get; set;}
	public Timer SlowTimer {get; set;}
	public Timer StunTimer {get; set;}
	public Timer FreezeTimer {get; set;}

	// private Dictionary statusTimers;
	public override void _Ready()
	{
		FireTimer = GetNodeOrNull<Timer>("FireTimer");
		SlowTimer = GetNodeOrNull<Timer>("SlowTimer");
		StunTimer = GetNodeOrNull<Timer>("StunTimer");
		FreezeTimer = GetNodeOrNull<Timer>("FreezeTimer");
		// statusTimers = new Dictionary();
		// foreach (Timer child in GetChildren()){
		// 	if (child is Timer){
		// 		statusTimers[child.Name] = child;
		// 		child.Timeout += () => {
		// 			if (child.Name == "OnFireTimer"){
		// 				IsOnFire = false;
		// 				EmitSignal(nameof(OnFire));
		// 				GD.Print("Off Fire");
		// 			}
		// 			if (child.Name == "OnFrozenTimer"){
		// 				IsFrozen = false;
		// 				GD.Print("Unfrozen");
		// 			}
		// 			if (child.Name == "OnStunnedTimer"){
		// 				IsStunned = false;
		// 				GD.Print("Unstunned");
		// 			}
		// 			if (child.Name == "OnSlowedTimer"){
		// 				IsSlowed = false;
		// 				GD.Print("Unslowed");
		// 			}
		// 		};

		// 	}
		// }
	}

	private void InitializeTimers(){
		FireTimer = GetNodeOrNull<Timer>("FireTimer");
		SlowTimer = GetNodeOrNull<Timer>("SlowTimer");
		StunTimer = GetNodeOrNull<Timer>("StunTimer");
		FreezeTimer = GetNodeOrNull<Timer>("FreezeTimer");

		FireTimer.Timeout += () => {
			IsOnFire = false;
			EmitSignal(nameof(OnFireTimeout));
		};

		SlowTimer.Timeout += () => {
			IsSlowed = false;
			EmitSignal(nameof(OnSlowedTimeout));
		};

		StunTimer.Timeout += () => {
			IsStunned = false;
			EmitSignal(nameof(OnStunnedTimeout));
		};

		FreezeTimer.Timeout += () => {
			IsFrozen = false;
			EmitSignal(nameof(OnFrozenTimeout));
		};
	}

	public void ApplyStatusEffect(Element firstElement, Element secondElement){
		if (firstElement == Element.Fire || secondElement == Element.Fire){
			IsOnFire = true;
			EmitSignal(nameof(OnFire));
			GD.Print("On Fire");
		}
		if (firstElement == Element.Water || secondElement == Element.Water){
			IsSlowed = true;
			GD.Print("Slowed");
		}
		if (firstElement == Element.Earth || secondElement == Element.Earth){
			//dodać obsługę stunów
			IsStunned = true;
			GD.Print("Stunned");
		}
	}
}
