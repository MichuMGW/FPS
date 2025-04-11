using Godot;
using System;
using Godot.Collections;

public partial class StatusComponent : Node
{
	//Można tu dodać float duration do sygnału
	// [Signal] public delegate void ApplyStatusEffectEventHandler(Element firstElement, Element secondElement);
	[Signal] public delegate void OnFireStartedEventHandler(float damage);
	[Signal] public delegate void OnFireEndedEventHandler();
	[Signal] public delegate void OnSlowStartedEventHandler(float slowAmount);
	[Signal] public delegate void OnSlowEndedEventHandler();
	[Signal] public delegate void OnStunnedEventHandler(bool isStunned);
	[Signal] public delegate void OnFrozenEventHandler(bool isFrozen);
	//TODO: Jakbym chciał dodać animacje przy spaleniu
	//[Signal] public delegate void OnBurnedEventHandler();
	public Timer FireTimer {get; set;}
	public Timer SlowTimer {get; set;}
	public Timer StunTimer {get; set;}
	public Timer FreezeTimer {get; set;}

	// private Dictionary statusTimers;
	public override void _Ready()
	{
		InitializeTimers();
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
		FireTimer = GetNode<Timer>("FireTimer");
		SlowTimer = GetNode<Timer>("SlowTimer");
		StunTimer = GetNode<Timer>("StunTimer");
		FreezeTimer = GetNode<Timer>("FreezeTimer");

		FireTimer.Timeout += () => {
			GD.Print("Off fire");
			EmitSignal(nameof(OnFireEnded));
		};

		SlowTimer.Timeout += () => {
			EmitSignal(nameof(OnSlowEnded));
		};

		StunTimer.Timeout += () => {
			EmitSignal(nameof(OnStunned), false);
		};

		FreezeTimer.Timeout += () => {
			EmitSignal(nameof(OnFrozen), false);
		};
	}

	public void ApplyStatusEffect(Element firstElement, Element secondElement){
		if (firstElement == Element.Fire || secondElement == Element.Fire){
			EmitSignal(nameof(OnFireStarted), 10f);
			FireTimer.Start();
			GD.Print("On Fire");
		}
		if (firstElement == Element.Water || secondElement == Element.Water){
			EmitSignal(nameof(OnSlowStarted), 0.5f);
			SlowTimer.Start();
			GD.Print("Slowed");
		}
		if (firstElement == Element.Earth || secondElement == Element.Earth){
			//dodać obsługę stunów
			EmitSignal(nameof(OnStunned), true);
			GD.Print("Stunned");
		}
	}
}
