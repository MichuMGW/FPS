using Godot;
using System;
using Godot.Collections;

public partial class StatusComponent : Node
{
	//Można tu dodać float duration do sygnału
	// [Signal] public delegate void ApplyStatusEffectEventHandler(Element firstElement, Element secondElement);
	[Signal] public delegate void FireStartedEventHandler(float damage);
	[Signal] public delegate void FireEndedEventHandler();
	[Signal] public delegate void SlowStartedEventHandler(float slowAmount);
	[Signal] public delegate void SlowEndedEventHandler();
	[Signal] public delegate void StunnedEventHandler(bool isStunned);
	[Signal] public delegate void FrozenEventHandler(bool isFrozen);
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
	}

	private void InitializeTimers(){
		FireTimer = GetNode<Timer>("FireTimer");
		SlowTimer = GetNode<Timer>("SlowTimer");
		StunTimer = GetNode<Timer>("StunTimer");
		FreezeTimer = GetNode<Timer>("FreezeTimer");

		FireTimer.Timeout += () => {
			GD.Print("Off fire");
			EmitSignal(nameof(FireEnded));
		};

		SlowTimer.Timeout += () => {
			EmitSignal(nameof(SlowEnded));
		};

		StunTimer.Timeout += () => {
			EmitSignal(nameof(Stunned), false);
		};

		FreezeTimer.Timeout += () => {
			EmitSignal(nameof(Frozen), false);
		};
	}

	public void ApplyStatusEffect(Element firstElement, Element secondElement){
		if (firstElement == Element.Fire || secondElement == Element.Fire){
			EmitSignal(nameof(FireStarted), 10f);
			FireTimer.Start();
			GD.Print("On Fire");
		}
		if (firstElement == Element.Water || secondElement == Element.Water){
			EmitSignal(nameof(SlowStarted), 0.5f);
			SlowTimer.Start();
			GD.Print("Slowed");
		}
		if (firstElement == Element.Earth || secondElement == Element.Earth){
			//dodać obsługę stunów
			EmitSignal(nameof(Stunned), true);
			GD.Print("Stunned");
		}
	}
}
