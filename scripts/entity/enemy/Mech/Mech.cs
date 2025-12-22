using Godot;
using System;
using System.Collections.Generic;

public partial class Mech : CharacterBody3D
{
    private const string L_IS_CHANNEL = "parameters/LeftHand/conditions/l_is_channel";
    private const string L_SHOOT      = "parameters/LeftHand/conditions/l_shoot";
    private const string R_IS_CHANNEL = "parameters/RightHand/conditions/r_is_channel";
    private const string R_IS_CHARGE  = "parameters/RightHand/conditions/r_is_charge";
    private const string R_RELEASE    = "parameters/RightHand/conditions/r_release";
    private const string R_SHOOT      = "parameters/RightHand/conditions/r_shoot";

    public VelocityComponent VelocityComp {get; private set; }
    public PathfindComponent Pathfind {get; private set; }
    public HealthComponent Health {get; private set; }
    public HurtboxComponent Hurtbox {get; private set; }
    public AnimationPlayer Animation {get; private set; }
    public AnimationTree AnimTree { get; private set; }
    private Skeleton3D _skeleton;
    public int _headBoneIndex {get; private set; }
    private AnimationNodeStateMachinePlayback _locomotionSm;
    private AnimationNodeStateMachinePlayback _upperBodySm;
    public Node3D Player {get; private set; }
    public Node3D PlayerAimTarget {get; private set; }
    [Export] public PackedScene BulletScene {get; private set; }
    [Export] public PackedScene RocketScene {get; private set; }
    [Export] public PackedScene LandingDecalScene { get; set; }
    private Node3D _leftBarrel;
    private Node3D _rightBarrel;
    private Node3D _leftLauncher;
    private Node3D _rightLauncher;
    public GpuParticles3D DebreesParticles {get; set; }
    
    

    [Export] public float MoveSpeed = 4f;
    [Export] public float CircleRadius = 10f;
    [Export] public float CircleAngularSpeed = 1.2f;

    [Export] public float GunCooldown = 2.0f;
    [Export] public float RocketCooldown = 6.0f;
    [Export] public float JumpCooldown = 30.0f;
    [Export] public float SpinCooldown = 30.0f;

    [Export] public float LandingDamageRadius = 20.0f;

     // ---------- FSM: SUPERSTATES ----------
    public MechSuperStateId CurrentSuperStateId { get; private set; }
    private IState _currentSuperState;
    private Dictionary<MechSuperStateId, IState> _superStates;

    // ---------- FSM: MOVEMENT ----------
    public MechMoveStateId CurrentMoveStateId { get; private set; }
    private IState _currentMoveState;
    private Dictionary<MechMoveStateId, IState> _moveStates;

    // ---------- FSM: ATTACK ----------
    public MechAttackStateId CurrentAttackStateId { get; private set; }
    private IState _currentAttackState;
    private Dictionary<MechAttackStateId, IState> _attackStates;

    [Export] public LookAtModifier3D LeftArmLookAt { get; set; }
    [Export] public LookAtModifier3D RightArmLookAt { get; set; }
    [Export] public LookAtModifier3D HeadLookAt { get; set; }
    private bool _lookAtActive = true;
    public bool LookAtActive
    {
        get => _lookAtActive;
        set
        {
            _lookAtActive = value;
        }
    }
    [Export] public float LookAtInfluenceSpeed { get; set; } = 4f;
    private float _currentGunCooldown;
    private float _currentRocketCooldown;
    private float _currentJumpCooldown;
    private float _currentSpinCooldown;
    private RandomNumberGenerator _rng = new();
    public override void _Ready()
    {
        FindNodes();
        SetAliveStateCollisions();
        LookAtActive = false;

        InitializeAnimationTree();

        var playerPath = PlayerAimTarget.GetPath();

        LeftArmLookAt.TargetNode = playerPath;
        RightArmLookAt.TargetNode = playerPath;
        HeadLookAt.TargetNode = playerPath;

        Health.EntityDied += OnEntityDied;
        
        InitializeSuperStates();
        InitializeMoveStates();
        InitializeAttackStates();

        ChangeMoveState(MechMoveStateId.ChasePlayer);
        ChangeAttackState(MechAttackStateId.None);
        ChangeSuperState(MechSuperStateId.Normal);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        _currentGunCooldown = MathF.Max(0, _currentGunCooldown - dt);
        _currentRocketCooldown = MathF.Max(0, _currentRocketCooldown - dt);
        _currentJumpCooldown = MathF.Max(0, _currentJumpCooldown - dt);
        _currentSpinCooldown = MathF.Max(0, _currentSpinCooldown - dt);

        _currentSuperState?.Update(delta);

         if (CurrentSuperStateId == MechSuperStateId.Normal)
        {
            _currentMoveState?.Update(delta);
            _currentAttackState?.Update(delta);
            HandleAiDecision(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentSuperState?.PhysicsUpdate(delta);

        if (CurrentSuperStateId == MechSuperStateId.Normal)
        {
            _currentMoveState?.PhysicsUpdate(delta);
            _currentAttackState?.PhysicsUpdate(delta);
        }

        HandleLookAtModifiers(delta);
    }

    private void InitializeSuperStates()
    {
        _superStates = new()
        {
            { MechSuperStateId.Normal, new MechSuperNormalState(this) },
            { MechSuperStateId.JumpSpecial, new MechJumpSpecialState(this) },
            { MechSuperStateId.SpinSpecial, new MechSpinSpecialState(this) },
            { MechSuperStateId.Dead, new MechDeadState(this) }
        };
    }

    private void InitializeMoveStates()
    {
        _moveStates = new()
        {
            { MechMoveStateId.Idle, new MechMoveIdleState(this) },
            { MechMoveStateId.ChasePlayer, new MechMoveChaseState(this) },
            { MechMoveStateId.CirclePlayer, new MechMoveCircleState(this) }
        };
    }

    private void InitializeAttackStates()
    {
        _attackStates = new()
        {
            { MechAttackStateId.None, new MechAttackNoneState(this) },
            { MechAttackStateId.GunBurst, new MechGunBurstState(this) },
            { MechAttackStateId.RocketVolley, new MechRocketVolleyState(this) }
        };
    }

    private void InitializeAnimationTree()
    {
        AnimTree.Active = true;

        _locomotionSm = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/Locomotion/playback");
        _upperBodySm = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/UpperBody/playback");

        PlayLocomotion("Mech_Idle");
        PlayUpperBody("Mech_Idle");
    }

    public void PlayLocomotion(string stateName)
    {
        _locomotionSm?.Travel(stateName);
    }

    public void PlayUpperBody(string stateName)
    {
        _upperBodySm?.Travel(stateName);
    }

    public void ResetUpperBody()
    {
        PlayUpperBody("Mech_Idle");
    }

    private void FindNodes()
    {
        VelocityComp = GetNode<VelocityComponent>("VelocityComponent");
        Pathfind = GetNode<PathfindComponent>("PathfindComponent");
        Health = GetNode<HealthComponent>("HealthComponent");
        Hurtbox = Health.Hurtbox;
        _skeleton = GetNode<Skeleton3D>("Mech/MechArmature/Skeleton3D");

        AnimTree = GetNode<AnimationTree>("Mech/AnimationTree");
        Animation = GetNode<AnimationPlayer>("Mech/AnimationPlayer");
        
        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        PlayerAimTarget = GetTree().GetFirstNodeInGroup("player_target") as Node3D;

        DebreesParticles = GetNode<GpuParticles3D>("Debrees");

        _leftBarrel = GetNode<Node3D>("Mech/MechArmature/Skeleton3D/ArmLeftAttachment/LeftBarrel");
        _rightBarrel = GetNode<Node3D>("Mech/MechArmature/Skeleton3D/ArmRightAttachment/RightBarrel");
        _leftLauncher = GetNode<Node3D>("Mech/MechArmature/Skeleton3D/ArmLeftAttachment/LeftLauncher");
        _rightLauncher = GetNode<Node3D>("Mech/MechArmature/Skeleton3D/ArmRightAttachment/RightLauncher");
    }


    public void ChangeSuperState(MechSuperStateId newState)
    {
        if (_currentSuperState != null && CurrentSuperStateId == newState)
            return;

        ChangeMoveState(MechMoveStateId.Idle);
        ChangeAttackState(MechAttackStateId.None);

        ResetUpperBody();
        _currentSuperState?.Exit();
        CurrentSuperStateId = newState;
        _currentSuperState = _superStates[newState];
        _currentSuperState.Enter();
    }

    public void ChangeMoveState(MechMoveStateId newState)
    {
        if (_currentMoveState != null && CurrentMoveStateId == newState)
            return;

        _currentMoveState?.Exit();
        CurrentMoveStateId = newState;
        _currentMoveState = _moveStates[newState];
        _currentMoveState.Enter();
    }

    public void ChangeAttackState(MechAttackStateId newState)
    {
        if (_currentAttackState != null && CurrentAttackStateId == newState)
            return;

        _currentAttackState?.Exit();
        CurrentAttackStateId = newState;
        _currentAttackState = _attackStates[newState];
        _currentAttackState.Enter();
    }

    public bool CanUseGun() => _currentGunCooldown <= 0;
    public bool CanUseRocket() => _currentRocketCooldown <= 0;
    public void ResetGunCooldown() => _currentGunCooldown = GunCooldown;
    public void ResetRocketCooldown() => _currentRocketCooldown = RocketCooldown;
    public void ResetJumpCooldown() => _currentJumpCooldown = JumpCooldown;
    public void ResetSpinCooldown() => _currentSpinCooldown = SpinCooldown;
    public void AddAfterSpecialCooldown()
    {
        _currentGunCooldown += 2f;
        _currentRocketCooldown += 2f;
        _currentJumpCooldown += 6f;
        _currentSpinCooldown += 6f;
    }

    private void HandleAiDecision(double delta)
    {
        if (Player == null) return;

        float distance = GlobalPosition.DistanceTo(Player.GlobalPosition);

        if (distance > CircleRadius * 1.3f)
        {
            ChangeMoveState(MechMoveStateId.ChasePlayer);
        }
        else if (distance < CircleRadius * 0.7f)
        {
            ChangeMoveState(MechMoveStateId.ChasePlayer);
        }
        else
        {
            ChangeMoveState(MechMoveStateId.CirclePlayer);
        }

        if (CurrentAttackStateId == MechAttackStateId.None)
        {
            TryTriggerSpecials();

            if (CurrentSuperStateId != MechSuperStateId.Normal)
                return;

            if (CanUseGun())
            {
                float roll = _rng.Randf();
                if (CanUseRocket() && roll < 0.25f)
                    ChangeAttackState(MechAttackStateId.RocketVolley);
                else
                    ChangeAttackState(MechAttackStateId.GunBurst);
            }
        }
    }

    private void TryTriggerSpecials()
    {
        if (CurrentSuperStateId != MechSuperStateId.Normal)
            return;

        float roll = _rng.Randf();

        if (_currentJumpCooldown <= 0 && roll < 0.10f)
        {
            ChangeSuperState(MechSuperStateId.JumpSpecial);
        }
        else if (_currentSpinCooldown <= 0 && roll < 0.20f)
        {
            ChangeSuperState(MechSuperStateId.SpinSpecial);
        }
    }

    //TODO: Dodać inicjalizację GPUParticles3D przy inicjalizacji pocisku
    public void SpawnGunProjectile(bool fromLeft)
    {
        var barrel = fromLeft ? _leftBarrel : _rightBarrel;

        var bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);

        bullet.GlobalTransform = barrel.GlobalTransform;

        Vector3 dir = barrel.GlobalTransform.Basis.Y;
        bullet.LookAt(bullet.GlobalPosition + dir, Vector3.Up);

        bullet.Initialize(dir);
    }

    //TODO: Dodać inicjalizację GPUParticles3D przy inicjalizacji rakiety
    public void SpawnRocket(bool fromLeft)
    {
        var launcher = fromLeft ? _leftBarrel : _rightBarrel;

        var rocket = RocketScene.Instantiate<Rocket>();
        GetTree().CurrentScene.AddChild(rocket);

        rocket.GlobalTransform = launcher.GlobalTransform;

        Vector3 dir = rocket.GlobalTransform.Basis.Y;
        rocket.LookAt(rocket.GlobalPosition + dir, Vector3.Up);

        rocket.Initialize(dir);
    }

    private void OnEntityDied()
    {
        SetDeadStateCollisions();
        ChangeSuperState(MechSuperStateId.Dead);
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

        private void HandleLookAtModifiers(double delta)
    {
        var influence = HeadLookAt.Influence;
        float influenceLerp;
        if (LookAtActive)
        {
            influenceLerp = Mathf.Lerp(influence, 1, LookAtInfluenceSpeed * (float)delta);
        }
        else
        {
            influenceLerp = Mathf.Lerp(influence, 0, LookAtInfluenceSpeed * (float)delta);
        }

        HeadLookAt.Influence = influenceLerp;
        LeftArmLookAt.Influence = influenceLerp;
        RightArmLookAt.Influence = influenceLerp;
    }
}
