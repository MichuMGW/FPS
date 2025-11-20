using Godot;
using System;
using System.Collections.Generic;

public partial class Orc : CharacterBody3D
{
    [Export] public VelocityComponent VelocityComp;
    [Export] public PathfindComponent Pathfind;
    [Export] public HealthComponent Health;
    [Export] public HurtboxComponent Hurtbox;
    [Export] public Node3D Player;
    [Export] public AnimationPlayer Animation;

    // KONFIG
    [Export] public float ChaseSpeed { get; set; } = 4f;
    public float BaseAcceleration { get; private set; }
    [Export] public float ChargeAcceleration { get; set; } = 4f; // coś mniejszego niż standardowe Acceleration
    [Export] public float ChargeCooldown {get; set; } = 3f;
    [Export] public float ChargeSpeed { get; set; } = 12f;
    [Export] public float ChargeTurnSpeed { get; set; } = 3f;     // jak szybko może skręcać w trakcie szarży
    [Export] public float MaxChargeTime { get; set; } = 2.5f;     // max czas szarży
    [Export] public float ObstacleCheckDistance { get; set; } = 3f;

    [Export] public float ChargeMinDistance { get; set; } = 6f;   // min dystans żeby miało sens szarżować
    [Export] public float ChargeMaxDistance { get; set; } = 18f;  // max dystans do szarży
    [Export] public float ChargeFovDotThreshold { get; set; } = 0.5f; // ~60° kąt widzenia

    // maska do raycastów "czy widzę gracza"
    [Export] public uint LineOfSightMask { get; set; } = (PhysicsLayers.TERRAIN) | (PhysicsLayers.PLAYER_BODY); 
    // 0 – świat, 1 – gracz (dobierz do swojego projektu)

    // maska przeszkód do wykrywania ściany przed szarżą
    [Export] public uint ObstacleMask { get; set; } = PhysicsLayers.TERRAIN; // np. WorldStatic

    private IState _currentState;
    private Dictionary<OrcStateId, IState> _states;

    public OrcStateId CurrentStateId { get; private set; }

    public override void _Ready()
    {
        VelocityComp ??= GetNode<VelocityComponent>("VelocityComponent");
        Pathfind ??= GetNode<PathfindComponent>("PathfindComponent");
        Health ??= GetNode<HealthComponent>("HealthComponent");
        Hurtbox ??= Health.Hurtbox;
        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;

        BaseAcceleration = VelocityComp.Acceleration;

        Health.EntityDied += OnEntityDied;

        _states = new Dictionary<OrcStateId, IState>
        {
            { OrcStateId.Chase,  new OrcChaseState(this) },
            { OrcStateId.Charge, new OrcChargeState(this) },
            { OrcStateId.Stop,   new OrcStopState(this) },
            { OrcStateId.Dead,   new OrcDeadState(this) },
        };

        ChangeState(OrcStateId.Chase);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public void ChangeState(OrcStateId newState)
    {
        if (CurrentStateId == newState)
            return;

        _currentState?.Exit();
        CurrentStateId = newState;
        _currentState = _states[newState];
        _currentState.Enter();
    }

    private void OnEntityDied()
    {
        ChangeState(OrcStateId.Dead);
    }
    // public VelocityComponent VelocityComp {get; private set; }
    // public PathfindComponent Pathfind {get; private set; }
    // public HealthComponent Health {get; private set; }
    // public HurtboxComponent Hurtbox {get; private set; }

    // public Node3D Player {get; private set; }
    // public Node3D PlayerAimTarget {get; private set; }

    // private Dictionary<OrcStateId, IState> _states;
    // private IState _currentState;
    // private OrcStateId _currentId;
    
    // public override void _Ready()
    // {
    //     FindNodes();
    //     SetAliveStateCollisions();

    //     Health.EntityDied += OnDied;

    //     _states = new()
    //     {
    //         // { OrcStateId.Idle,  new OrcIdleState(this) },
    //         { OrcStateId.Chase, new OrcChaseState(this) },
    //         { OrcStateId.Charge, new OrcChargeState(this) },
    //         { OrcStateId.Stop, new OrcStopState(this) },
    //         { OrcStateId.Dead,  new OrcDeadState(this) },
    //     };

    //     ChangeState(OrcStateId.Chase);
    // }
    // private void OnDied()
    // {
    //     SetDeadStateCollisions();

    //     Pathfind.Active = false;
    //     VelocityComp.Active = false;

    //     ChangeState(OrcStateId.Dead);
    // }

    // public void ChangeState(OrcStateId newId)
    // {
    //     if (newId == _currentId) return;

    //     Arrow.Visible = false;
    //     _currentState?.Exit();
    //     _currentId = newId;
    //     _currentState = _states[newId];
    //     _currentState.Enter();
    // }

    // public override void _Process(double delta)
    // {
    //     _currentState?.Update(delta);
    // }

    // public override void _PhysicsProcess(double delta)
    // {
    //     _currentState?.PhysicsUpdate(delta);
    // }

    // private void FindNodes()
    // {
    //     VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
    //     Pathfind = GetNode<PathfindComponent>("PathfindComponent");
    //     Health = GetNode<HealthComponent>("HealthComponent");

    //     Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    //     PlayerAimTarget = GetTree().GetFirstNodeInGroup("player_target") as Node3D;
    // }

    // private void SetAliveStateCollisions()
    // {
    //     CollisionLayer = PhysicsLayers.ENEMY_BODY;
    //     CollisionMask = PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY | PhysicsLayers.TERRAIN;
    // }
    // private void SetDeadStateCollisions()
    // {
    //     CollisionMask = PhysicsLayers.TERRAIN;
    // }
}
