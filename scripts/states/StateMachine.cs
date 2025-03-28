using Godot;
using System;
using Godot.Collections;

public partial class StateMachine : Node
{
    [Export] public State initialState;
    private Dictionary states = new Dictionary();
    private State currentState;

    public override void _Ready()
    {
        foreach(State child in GetChildren()){
            if (child is State){
                GD.Print("Adding child state: " + child.Name);
                states[child.Name] = child;
                child.Transitioned += OnChildTransition;
            }
        }

        if (initialState != null){
            initialState.Enter();
            currentState = initialState;
        }
    }

    public override void _Process(double delta)
    {
        if (currentState != null){
            currentState.Update(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (currentState != null){
            currentState.Physics_Update(delta);
        }
    }

    private void OnChildTransition(State state, string newStateName)
    {
        if (state != currentState){
            return;
        }

        State newState = (State)states[newStateName];

        if (newState == null){
            return;
        }

        if (currentState != null){
            currentState.Exit();
        }

        currentState = newState;
        newState.Enter();
    }
}
