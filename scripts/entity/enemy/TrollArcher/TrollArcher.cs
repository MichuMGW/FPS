using Godot;
using System;
using System.Collections.Generic;

public partial class TrollArcher : CharacterBody3D
{
    

    
    [Export] public PackedScene ArrowProjectileScene { get; private set; }
    [Export] public PackedScene BowScene {get; private set; }
    [Export] public float DrawRotationSpeed {get; private set; } = 4f;
    [Export] public float AimTime {get; private set; } = 1.5f;
    
    [Export] public float SpineLookSmoothSpeed {get; private set; } = 10f;

    public VelocityComponent VelocityComp {get; private set; }
    public PathfindComponent Pathfind {get; private set; }
    public HealthComponent Health {get; private set; }
    public HurtboxComponent Hurtbox {get; private set; }

    public AnimationPlayer TrollAnimation {get; private set; }
    public AnimationPlayer BowAnimation {get; private set; }
    public AnimationPlayer ArrowAnimation {get; private set; }

    public Node3D ArrowSpawnPoint { get; private set; }
    public Node3D AimTarget {get; set; }
    public LookAtModifier3D SpineLookAt {get; set; }
    public Node3D Player {get; private set; }
    public Node3D PlayerAimTarget {get; private set; }
    public Node3D Bow {get; set; }
    public Node3D Arrow {get; set; }

    private Dictionary<TrollStateId, IState> _states;
    private IState _currentState;
    private TrollStateId _currentId;

    [Export] public float ShootDistance {get; private set;} = 15f;

    public override void _Ready()
    {
        FindNodes();
        SetAliveStateCollisions();

        Health.EntityDied += OnDied;

        _states = new()
        {
            { TrollStateId.Idle,  new TrollIdleState(this) },
            { TrollStateId.Chase, new TrollChaseState(this) },
            { TrollStateId.Draw,  new TrollDrawState(this) },
            { TrollStateId.Shoot, new TrollShootState(this) },
            { TrollStateId.Dead,  new TrollDeadState(this) },
        };

        ChangeState(TrollStateId.Chase);
    }

    private void FindNodes()
    {   
        VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
        Pathfind = GetNode<PathfindComponent>("PathfindComponent");
        Health = GetNode<HealthComponent>("HealthComponent");

        TrollAnimation = GetNode<AnimationPlayer>("troll_archer/AnimationPlayer");
        BowAnimation = GetNode<AnimationPlayer>("troll_archer/TrollArcherRig/Skeleton3D/LeftHandAttachment/bow/AnimationPlayer");
        ArrowAnimation = GetNode<AnimationPlayer>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/arrow/AnimationPlayer");

        ArrowSpawnPoint = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/ArrowSpawnPoint");
        AimTarget = GetNode<Node3D>("AimTarget");
        Bow = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/LeftHandAttachment/bow");
        Arrow = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/arrow");

        SpineLookAt = GetNode<LookAtModifier3D>("troll_archer/TrollArcherRig/Skeleton3D/SpineLookAtModifier3D");

        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        PlayerAimTarget = GetTree().GetFirstNodeInGroup("player_target") as Node3D;

    }

    public void RotateHorizontallyTowardsPlayer(float delta)
    {
        if (Player == null)
            return;

        var toPlayer = Player.GlobalPosition - GlobalPosition;
        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() < 0.001f)
        {
            return;    
        }

        var desiredDir = toPlayer.Normalized();
        var currentForward = GlobalTransform.Basis.Z;
        var newForward = currentForward.Slerp(desiredDir, DrawRotationSpeed * delta).Normalized();
        var targetPos = GlobalPosition - newForward;

        LookAt(targetPos, Vector3.Up);
    }

    private void OnDied()
    {
        DisableSpineLookAtTarget();
        SetDeadStateCollisions();

        Pathfind.Active = false;
        VelocityComp.Active = false;

        ChangeState(TrollStateId.Dead);
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

    public void ChangeState(TrollStateId newId)
    {
        if (newId == _currentId) return;

        Arrow.Visible = false;
        _currentState?.Exit();
        _currentId = newId;
        _currentState = _states[newId];
        _currentState.Enter();
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public void SetSpineLookAtPlayer()
    {
        SpineLookAt.TargetNode = PlayerAimTarget.GetPath();
    }

    public void SetSpineLookAtAimTarget()
    {
        SpineLookAt.TargetNode = AimTarget.GetPath();
    }

    public void EnableSpineLookAtTarget()
    {
        SpineLookAt.Active = true;
    }

    public void DisableSpineLookAtTarget()
    {
        SpineLookAt.Active = false;
    }

    public void UpdateSpineLookTarget(float delta)
    {
        if (Player == null || PlayerAimTarget == null)
            return;
        
        if(SpineLookAt.IsTargetWithinLimitation())
        {
            AimTarget.GlobalPosition = PlayerAimTarget.GlobalPosition;
        }   
    }
}

