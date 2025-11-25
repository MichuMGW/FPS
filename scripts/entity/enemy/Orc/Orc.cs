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

    // KONFIG
    [Export] public float ChaseSpeed { get; set; } = 4f;
    public float BaseAcceleration { get; private set; }
    [Export] public float ChargeAcceleration { get; set; } = 4f; // coś mniejszego niż standardowe Acceleration
    [Export] public float ChargeCooldown {get; set; } = 3f;
    [Export] public float ChargeSpeed { get; set; } = 12f;
    [Export] public float ChargeTurnSpeed { get; set; } = 2f;     // jak szybko może skręcać w trakcie szarży
    [Export] public float MaxChargeTime { get; set; } = 2.5f;     // max czas szarży
    [Export] public float ObstacleCheckDistance { get; set; } = 3f;

    [Export] public float ChargeMinDistance { get; set; } = 6f;   // min dystans żeby miało sens szarżować
    [Export] public float ChargeMaxDistance { get; set; } = 18f;  // max dystans do szarży
    [Export] public float ChargeFovDotThreshold { get; set; } = 0.5f; // ~60° kąt widzenia
    // łagodny ramp-up prędkości maksymalnej z ChaseSpeed -> ChargeSpeed
    // ChargeRampSpeed: jak szybko dobijamy do pełnej prędkości szarży
    [Export] public float ChargeRampSpeed {get; private set; } = 2f;
    [Export] public float ChargeDamage { get; set; } = 20f;
    [Export] public float ChargeKnockbackForce { get; set; } = 15f;

    [Export] public uint LineOfSightMask { get; set; } = (PhysicsLayers.TERRAIN) | (PhysicsLayers.PLAYER_BODY); 

    [Export] public uint ObstacleMask { get; set; } = PhysicsLayers.TERRAIN; // np. WorldStatic
    public AnimationPlayer Animation;
    public Area3D ChargeHitbox;

    private IState _currentState;
    private Dictionary<OrcStateId, IState> _states;

    public OrcStateId CurrentStateId { get; private set; }

    public override void _Ready()
    {
        FindNodes();
        SetAliveStateCollisions();

        BaseAcceleration = VelocityComp.Acceleration;

        Health.EntityDied += OnEntityDied;
        ChargeHitbox.BodyEntered += OnChargeHitboxBodyEntered;

        _states = new Dictionary<OrcStateId, IState>
        {
            { OrcStateId.Chase,  new OrcChaseState(this) },
            { OrcStateId.Charge, new OrcChargeState(this) },
            { OrcStateId.Stop,   new OrcStopState(this) },
            { OrcStateId.Dead,   new OrcDeadState(this) },
        };

        ChangeState(OrcStateId.Chase);
    }

    private void FindNodes()
    {
        VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
        Pathfind = GetNode<PathfindComponent>("PathfindComponent");
        Health = GetNode<HealthComponent>("HealthComponent");
        Hurtbox = Health.Hurtbox;
        Animation = GetNode<AnimationPlayer>("orc/AnimationPlayer");
        ChargeHitbox = GetNode<Area3D>("ChargeHitbox");

        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
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
        SetDeadStateCollisions();
        ChangeState(OrcStateId.Dead);
    }

    private void OnChargeHitboxBodyEntered(Node3D body)
    {
        if (body is Player player)
        {
            // Kierunek knockbacku
            Vector3 dir = (player.GlobalPosition - GlobalPosition);
            dir.Y = 0.01f; //lot w pionie
            dir = dir.Normalized();

            // 2) Zadaj obrażenia graczowi
            // player.Health.TakeDamage(ChargeDamage);

            player.Knockback.ApplyKnockback(dir * ChargeKnockbackForce);

            ChangeState(OrcStateId.Stop);

        }
    }

    private void SetAliveStateCollisions()
    {
        CollisionLayer = PhysicsLayers.ENEMY_BODY;
        CollisionMask = PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY | PhysicsLayers.TERRAIN;
    }
    private void SetDeadStateCollisions()
    {
        CollisionMask = PhysicsLayers.TERRAIN;
    }

}
