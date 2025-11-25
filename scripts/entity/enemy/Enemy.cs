using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Enemy<TState> : CharacterBody3D where TState : Enum
{
    public VelocityComponent VelocityComp { get; protected set; }
    public PathfindComponent Pathfind { get; protected set; }
    public HealthComponent Health { get; protected set; }
    public HurtboxComponent Hurtbox { get; protected set; }
    public AnimationPlayer Animation { get; protected set; }
    public Node3D Player { get; protected set; }

    protected Dictionary<TState, IState> _states;
    protected IState _currentState;
    public TState CurrenTState { get; private set; }

    public override void _Ready()
    {
        FindCommonNodes();

        Health.EntityDied += OnEntityDied;

        _states = new Dictionary<TState, IState>();
        InitializeStates();

        ChangeState(GetDefaulTState());
    }

    protected virtual void FindCommonNodes()
    {
        VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
        Pathfind = GetNode<PathfindComponent>("PathfindComponent");
        Health = GetNode<HealthComponent>("HealthComponent");
        Hurtbox = Health.Hurtbox;

        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }

    protected abstract void InitializeStates();
    protected abstract TState GetDefaulTState();

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public void ChangeState(TState newState)
    {
        if (EqualityComparer<TState>.Default.Equals(CurrenTState, newState))
            return;

        _currentState?.Exit();
        CurrenTState = newState;
        _currentState = _states[newState];
        _currentState.Enter();
    }

    protected virtual void OnEntityDied()
    {
        // Domyślnie próbujemy przejść do Dead, jeżeli istnieje
        if (_states.ContainsKey((TState)(object)Enum.Parse(typeof(TState), "Dead")))
        {
            ChangeState((TState)(object)Enum.Parse(typeof(TState), "Dead"));
        }
        else
        {
            QueueFree();
        }
    }
}