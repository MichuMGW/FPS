using Godot;
using System;

public abstract partial class State : Node
{
	[Signal] public delegate void TransitionedEventHandler(State state, string newStateName);	//Signal emmited when there is state transition
	public abstract void Enter();	//Called when state is entered
	public abstract void Exit();	//Called when state is exited
	public abstract void Update(double delta);	//Called every frame
	public abstract void Physics_Update(double delta);	//Called every physics frame
}
