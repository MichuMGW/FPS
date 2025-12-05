using Godot;
using System;
using System.Collections.Generic;

public partial class Skeleton : CharacterBody3D
{
    public VelocityComponent VelocityComp {get; private set; }
    public PathfindComponent Pathfind {get; private set; }
    public HealthComponent Health {get; private set; }
    public HurtboxComponent Hurtbox {get; private set; }
    public AnimationPlayer Animation {get; private set; }
    public Node3D Player {get; private set; }

    //TODO: Zastąpić AttackHitbox -> HitboxComponent
    public Area3D AttackHitbox {get; set;}
    public float AttackDistance {get; set;} = 2f;
    [Export] public float Speed {get; set;} = 10f;

    private IState _currentState;
    private Dictionary<SkeletonStateId, IState> _states;
    public SkeletonStateId CurrentStateId { get; private set; }
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    public override void _Ready()
    {
        FindNodes();
        SetAliveStateCollisions();

        AttackHitbox.Monitoring = false;
        VelocityComp.MaxSpeed = Speed;

        Health.EntityDied += OnEntityDied;

        _states = new Dictionary<SkeletonStateId, IState>
        {
            { SkeletonStateId.Spawn,  new SkeletonSpawnState(this) },
            { SkeletonStateId.Chase,  new SkeletonChaseState(this) },
            { SkeletonStateId.Attack, new SkeletonAttackState(this) },
            { SkeletonStateId.Dead,   new SkeletonDeadState(this) },
        };

        ChangeState(SkeletonStateId.Spawn);
    }

    private void FindNodes()
    {
        VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
        Pathfind = GetNode<PathfindComponent>("PathfindComponent");
        Health = GetNode<HealthComponent>("HealthComponent");
        Hurtbox = Health.Hurtbox;
        AttackHitbox = GetNode<Area3D>("AttackHitbox");
        Animation = GetNode<AnimationPlayer>("skeleton/AnimationPlayer");
        
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

    public void ChangeState(SkeletonStateId newState)
    {
        if (_currentState != null && CurrentStateId == newState)
            return;

        _currentState?.Exit();
        CurrentStateId = newState;
        _currentState = _states[newState];
        _currentState.Enter();
    }

    private void OnEntityDied()
    {
        SetDeadStateCollisions();
        ChangeState(SkeletonStateId.Dead);
    }

    public void PlayAnimationRandomized(StringName animName, bool randomizeTime = false)
    {
        if (!Animation.HasAnimation(animName))
        {
            return;
        }

        Animation.Play(animName, 0.3f);

        float length = Animation.GetAnimation(animName).Length;
        if(randomizeTime) {
            var randomTime = _rng.RandfRange(0f, length);
            Animation.Seek(randomTime, true);
        }

        Animation.SpeedScale = _rng.RandfRange(0.9f, 1.1f);
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
