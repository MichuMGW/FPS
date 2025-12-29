using Godot;
using System;
using System.Collections.Generic;

public partial class TrollArcher : StateMachineEnemy<TrollStateId>
{
    [Export] public PackedScene ArrowProjectileScene { get; private set; }
    [Export] public PackedScene BowScene {get; private set; }
    [Export] public float DrawRotationSpeed {get; private set; } = 4f;
    [Export] public float AimTime {get; private set; } = 1.5f;
    
    [Export] public float SpineLookSmoothSpeed {get; private set; } = 10f;
    [Export] public float ShootDistance {get; private set;} = 15f;

    public AnimationPlayer TrollAnimation {get; private set; }
    public AnimationPlayer BowAnimation {get; private set; }
    public AnimationPlayer ArrowAnimation {get; private set; }

    public LookAtModifier3D SpineLookAt {get; set; }

    public Node3D ArrowSpawnPoint { get; private set; }
    public Node3D AimTarget {get; set; }
    public Node3D Bow {get; set; }
    public Node3D Arrow {get; set; }
    private TrollStateId _currentId;

    protected override void FindNodes()
    {   
        base.FindNodes();

        TrollAnimation = GetNode<AnimationPlayer>("troll_archer/AnimationPlayer");
        BowAnimation = GetNode<AnimationPlayer>("troll_archer/TrollArcherRig/Skeleton3D/LeftHandAttachment/bow/AnimationPlayer");
        ArrowAnimation = GetNode<AnimationPlayer>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/arrow/AnimationPlayer");

        ArrowSpawnPoint = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/ArrowSpawnPoint");
        AimTarget = GetNode<Node3D>("AimTarget");
        Bow = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/LeftHandAttachment/bow");
        Arrow = GetNode<Node3D>("troll_archer/TrollArcherRig/Skeleton3D/RightHandAttachment/arrow");

        SpineLookAt = GetNode<LookAtModifier3D>("troll_archer/TrollArcherRig/Skeleton3D/SpineLookAtModifier3D");
    }

    protected override void OnAfterReady()
    {
        Arrow.Visible = false;

        States = new()
        {
            { TrollStateId.Idle, new TrollIdleState(this) },
            { TrollStateId.Chase, new TrollChaseState(this) },
            { TrollStateId.Draw, new TrollDrawState(this) },
            { TrollStateId.Shoot, new TrollShootState(this) },
            { TrollStateId.Dead, new TrollDeadState(this) },
        };

        ChangeState(TrollStateId.Chase);
    }

    protected override void OnDied()
    {
        if (SpineLookAt != null)
            SpineLookAt.Active = false;

        ChangeState(TrollStateId.Dead);
    }

    public void ShootArrow()
    {
        if (ArrowProjectileScene == null || ArrowSpawnPoint == null || AimTarget == null)
            return;

        var arrow = ArrowProjectileScene.Instantiate<ArrowProjectile>();
        arrow.Damage = Damage;
        arrow.GlobalTransform = ArrowSpawnPoint.GlobalTransform;

        Vector3 dir = (AimTarget.GlobalPosition - ArrowSpawnPoint.GlobalPosition).Normalized();
        arrow.Velocity = dir * arrow.Speed;

        GetTree().CurrentScene.AddChild(arrow);
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

