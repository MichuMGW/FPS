using Godot;
using System;
using System.Collections.Generic;

public partial class Mech : CharacterBody3D
{
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
    public GpuParticles3D DebreesPrtcl {get; set; }
    
    

    [Export] public float MoveSpeed = 4f;
    [Export] public float CircleRadius = 10f;
    [Export] public float CircleAngularSpeed = 1.2f;

    [Export] public float GunAttackCooldown = 2.0f;
    [Export] public float RocketAttackCooldown = 6.0f;
    [Export] public float JumpSpecialCooldown = 30.0f;
    [Export] public float SpinSpecialCooldown = 30.0f;

    [Export] public float LandingDamageRadius = 20.0f;

     // ---------- FSM: SUPER ----------
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


    // [Export] public float Speed {get; set;} = 10f;

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
            // SetLookAtModifiers(value);
        }
    }
    [Export] public float LookAtInfluenceSpeed { get; set; } = 4f;
    private float _gunCooldown;
    private float _rocketCooldown;
    private float _jumpCooldown;
    private float _spinCooldown;
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

        // ChangeSuperState(MechSuperStateId.Normal);
        // ChangeMoveState(MechMoveStateId.ChasePlayer);
        // ChangeAttackState(MechAttackStateId.None);
        ChangeMoveState(MechMoveStateId.Idle);
        ChangeAttackState(MechAttackStateId.RocketVolley);
        ChangeSuperState(MechSuperStateId.Normal);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        _gunCooldown = MathF.Max(0, _gunCooldown - dt);
        _rocketCooldown = MathF.Max(0, _rocketCooldown - dt);
        _jumpCooldown = MathF.Max(0, _jumpCooldown - dt);
        _spinCooldown = MathF.Max(0, _spinCooldown - dt);

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

        DebreesPrtcl = GetNode<GpuParticles3D>("Debrees");

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

    private void HandleAiDecision(double delta)
    {
        if (Player == null) return;

        float distance = GlobalPosition.DistanceTo(Player.GlobalPosition);
        // Ruch: prosty przykład – bliżej -> krąży, dalej -> goni
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

        // Ataki podstawowe – jeżeli nic nie robi
        if (CurrentAttackStateId == MechAttackStateId.None)
        {
            // najpierw sprawdzamy, czy nie odpalić specjali
            TryTriggerSpecials();

            // jeśli mimo wszystko dalej Normal + brak specjali → wybór ataku
            if (CurrentSuperStateId != MechSuperStateId.Normal)
                return;

            // priorytet: Gun częściej niż Rocket
            if (CanUseGun())
            {
                // szansa na rakietę zamiast pocisków
                float roll = _rng.Randf();
                if (CanUseRocket() && roll < 0.25f)
                    ChangeAttackState(MechAttackStateId.RocketVolley);
                else
                    // ChangeAttackState(MechAttackStateId.GunBurst);
                    ChangeAttackState(MechAttackStateId.RocketVolley);
            }
        }
    }

    private void TryTriggerSpecials()
    {
        if (CurrentSuperStateId != MechSuperStateId.Normal)
            return;

        float roll = _rng.Randf();

        if (_jumpCooldown <= 0 && roll < 0.10f)
        {
            ChangeSuperState(MechSuperStateId.JumpSpecial);
        }
        else if (_spinCooldown <= 0 && roll < 0.20f)
        {
            ChangeSuperState(MechSuperStateId.SpinSpecial);
        }
    }

    public bool CanUseGun() => _gunCooldown <= 0;
    public bool CanUseRocket() => _rocketCooldown <= 0;
    public void ResetGunCooldown() => _gunCooldown = GunAttackCooldown;
    public void ResetRocketCooldown() => _rocketCooldown = RocketAttackCooldown;
    public void ResetJumpCooldown() => _jumpCooldown = JumpSpecialCooldown;
    public void ResetSpinCooldown() => _spinCooldown = SpinSpecialCooldown;
    public void AddAfterSpecialCooldown()
    {
        _gunCooldown += 2f;
        _rocketCooldown += 2f;
        _jumpCooldown += 6f;
        _spinCooldown += 6f;
    }

    // Tu dodasz metody do spawnów pocisków / rakiet / efektów itd.
    public void SpawnGunProjectile(bool fromLeft)
    {
        GD.Print($"[Mech] Shoot gun, fromLeft={fromLeft}");
        // TODO: instancja pocisku + GPU particles
        var barrel = fromLeft ? _leftBarrel : _rightBarrel;

        var bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);

        bullet.GlobalTransform = barrel.GlobalTransform;

        Vector3 dir = barrel.GlobalTransform.Basis.Y;
        bullet.LookAt(bullet.GlobalPosition + dir, Vector3.Up);

        bullet.Initialize(dir);
    }

    public void SpawnRocket(bool fromLeft)
    {
        GD.Print($"[Mech] Shoot ROCKET, fromLeft={fromLeft}");
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

    public void RotateHeadBone(double delta)
    {
        float rotationDegreesPerSecond = 90.0f;
            float rotationAmount = rotationDegreesPerSecond * (float)delta;

            Quaternion currentRotation = _skeleton.GetBonePoseRotation(_headBoneIndex);
            
            Quaternion deltaRotation = Quaternion.FromEuler(new Vector3(
                0, 
                Mathf.DegToRad(rotationAmount), // Obrót wokół osi Y
                0
            ));

            // 3. Połącz obecną rotację z dodatkowym obrotem (deltaRotation)
            // Mnożenie kwaternionów to kompozycja obrotów.
            Quaternion newRotation = currentRotation * deltaRotation;

            // 4. Zastosuj nową rotację pozie kości
            _skeleton.SetBonePoseRotation(_headBoneIndex, newRotation);
    }

    // public void ClearTorsoSpinOverride()
    // {
    //     if (_skeleton == null || _headBoneIndex < 0)
    //         return;

    //     // wyłączenie override'u
    //     Skeleton.SetBonePoseRotation(_headBoneIndex, Transform3D.Identity, 0.0f, false);
    // }
}
