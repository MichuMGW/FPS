using Godot;
using System;
using System.Collections.Generic;

public partial class Mech : Enemy
{
    // ---------- Anim / rig ----------
    public AnimationPlayer Animation { get; private set; }
    public AnimationTree AnimTree { get; private set; }
    private Skeleton3D _skeleton;
    public int _headBoneIndex { get; private set; }

    private AnimationNodeStateMachinePlayback _locomotionSm;
    private AnimationNodeStateMachinePlayback _upperBodySm;

    [Export] public PackedScene BulletScene { get; private set; }
    [Export] public PackedScene RocketScene { get; private set; }
    [Export] public PackedScene LandingDecalScene { get; set; }

    private Node3D _leftBarrel;
    private Node3D _rightBarrel;
    private Node3D _leftLauncher;
    private Node3D _rightLauncher;

    public GpuParticles3D DebreesParticles { get; private set; }

    // ---------- Boss params ----------
    [Export] public float CircleRadius = 10f;
    [Export] public float CircleAngularSpeed = 1.2f;

    [Export] public float GunCooldown = 2.0f;
    [Export] public float RocketCooldown = 6.0f;
    [Export] public float JumpCooldown = 30.0f;
    [Export] public float SpinCooldown = 30.0f;

    [Export] public float LandingDamageRadius = 20.0f;

    // ---------- LookAt ----------
    [Export] public LookAtModifier3D LeftArmLookAt { get; set; }
    [Export] public LookAtModifier3D RightArmLookAt { get; set; }
    [Export] public LookAtModifier3D HeadLookAt { get; set; }

    [Export] public float LookAtInfluenceSpeed { get; set; } = 4f;

    private bool _lookAtActive;
    public bool LookAtActive
    {
        get => _lookAtActive;
        set
        {
            _lookAtActive = value;
        }
    }

    // ---------- Cooldowns ----------
    private float _currentGunCooldown;
    private float _currentRocketCooldown;
    private float _currentJumpCooldown;
    private float _currentSpinCooldown;

    private readonly RandomNumberGenerator _rng = new();

    // ---------- FSM (3 regiony) ----------
    public MechSuperStateId CurrentSuperStateId => _super.CurrentId;
    public MechMoveStateId CurrentMoveStateId => _move.CurrentId;
    public MechAttackStateId CurrentAttackStateId => _attack.CurrentId;

    private StateSlot<MechSuperStateId> _super;
    private StateSlot<MechMoveStateId> _move;
    private StateSlot<MechAttackStateId> _attack;

    // ===================== Enemy hooks =====================

    protected override void FindNodes()
    {
        base.FindNodes();

        // Uwaga: bazowe komponenty (VelocityComp/Pathfind/Health/Hurtbox/Player/Target) masz już z Enemy

        _skeleton = GetNodeOrNull<Skeleton3D>("Mech/MechArmature/Skeleton3D");

        AnimTree = GetNodeOrNull<AnimationTree>("Mech/AnimationTree");
        Animation = GetNodeOrNull<AnimationPlayer>("Mech/Mech/AnimationPlayer")
                    ?? GetNodeOrNull<AnimationPlayer>("Mech/AnimationPlayer"); // na wypadek innej ścieżki

        DebreesParticles = GetNodeOrNull<GpuParticles3D>("Debrees");

        _leftBarrel = GetNodeOrNull<Node3D>("Mech/MechArmature/Skeleton3D/ArmLeftAttachment/LeftBarrel");
        _rightBarrel = GetNodeOrNull<Node3D>("Mech/MechArmature/Skeleton3D/ArmRightAttachment/RightBarrel");
        _leftLauncher = GetNodeOrNull<Node3D>("Mech/MechArmature/Skeleton3D/ArmLeftAttachment/LeftLauncher");
        _rightLauncher = GetNodeOrNull<Node3D>("Mech/MechArmature/Skeleton3D/ArmRightAttachment/RightLauncher");
    }

    protected override void OnAfterReady()
    {

        LookAtActive = false;

        InitializeAnimationTree();
        SetupLookAtTargets();

        InitializeStateMachines();

        // start jak wcześniej
        ChangeMoveState(MechMoveStateId.ChasePlayer);
        ChangeAttackState(MechAttackStateId.None);
        ChangeSuperState(MechSuperStateId.Normal);
    }

    protected override void TickBrain(float dt)
    {
        // cooldowny (wcześniej były w _Process)
        _currentGunCooldown = MathF.Max(0, _currentGunCooldown - dt);
        _currentRocketCooldown = MathF.Max(0, _currentRocketCooldown - dt);
        _currentJumpCooldown = MathF.Max(0, _currentJumpCooldown - dt);
        _currentSpinCooldown = MathF.Max(0, _currentSpinCooldown - dt);

        // Super zawsze
        _super?.TickPhysics(dt);
        _super?.TickUpdate(dt);

        // Dwa regiony tylko w trybie normal
        if (CurrentSuperStateId == MechSuperStateId.Normal)
        {
            _move?.TickPhysics(dt);
            _move?.TickUpdate(dt);

            _attack?.TickPhysics(dt);
            _attack?.TickUpdate(dt);

            HandleAiDecision(dt);
        }

        // lookat (wcześniej w _PhysicsProcess)
        HandleLookAtModifiers(dt);
    }

    protected override void OnDied()
    {
        // bazowy Enemy już:
        // - wyłączył movement
        // - zmienił kolizje dead
        // tu robisz bossową logikę
        ChangeSuperState(MechSuperStateId.Dead);
    }

    // ===================== Init =====================

    private void InitializeAnimationTree()
    {
        if (AnimTree == null)
            return;

        AnimTree.Active = true;

        _locomotionSm = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/Locomotion/playback");
        _upperBodySm = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/UpperBody/playback");

        PlayLocomotion("Mech_Idle");
        PlayUpperBody("Mech_Idle");
    }

    private void SetupLookAtTargets()
    {
        if (PlayerAimTarget == null)
            return;

        var playerPath = PlayerAimTarget.GetPath();

        if (LeftArmLookAt != null) LeftArmLookAt.TargetNode = playerPath;
        if (RightArmLookAt != null) RightArmLookAt.TargetNode = playerPath;
        if (HeadLookAt != null) HeadLookAt.TargetNode = playerPath;
    }

    private void InitializeStateMachines()
    {
        var superStates = new Dictionary<MechSuperStateId, IState>
        {
            { MechSuperStateId.Normal, new MechSuperNormalState(this) },
            { MechSuperStateId.JumpSpecial, new MechJumpSpecialState(this) },
            { MechSuperStateId.SpinSpecial, new MechSpinSpecialState(this) },
            { MechSuperStateId.Dead, new MechDeadState(this) }
        };
        _super = new StateSlot<MechSuperStateId>(superStates);

        var moveStates = new Dictionary<MechMoveStateId, IState>
        {
            { MechMoveStateId.Idle, new MechMoveIdleState(this) },
            { MechMoveStateId.ChasePlayer, new MechMoveChaseState(this) },
            { MechMoveStateId.CirclePlayer, new MechMoveCircleState(this) }
        };
        _move = new StateSlot<MechMoveStateId>(moveStates);

        var attackStates = new Dictionary<MechAttackStateId, IState>
        {
            { MechAttackStateId.None, new MechAttackNoneState(this) },
            { MechAttackStateId.GunBurst, new MechGunBurstState(this) },
            { MechAttackStateId.RocketVolley, new MechRocketVolleyState(this) }
        };
        _attack = new StateSlot<MechAttackStateId>(attackStates);
    }

    // ===================== FSM API =====================

    public void ChangeSuperState(MechSuperStateId newState)
    {
        if (_super?.Current != null && CurrentSuperStateId == newState)
            return;

        // reset regionów przy wejściu w special/dead, jak miałeś
        ChangeMoveState(MechMoveStateId.Idle);
        ChangeAttackState(MechAttackStateId.None);

        ResetUpperBody();

        _super.Change(newState);
    }

    public void ChangeMoveState(MechMoveStateId newState)
    {
        _move.Change(newState);
    }

    public void ChangeAttackState(MechAttackStateId newState)
    {
        _attack.Change(newState);
    }

    // ===================== Anim helpers =====================

    public void PlayLocomotion(string stateName) => _locomotionSm?.Travel(stateName);
    public void PlayUpperBody(string stateName) => _upperBodySm?.Travel(stateName);
    public void ResetUpperBody() => PlayUpperBody("Mech_Idle");

    // ===================== Cooldowns / decisions =====================

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
            ChangeMoveState(MechMoveStateId.ChasePlayer);
        else if (distance < CircleRadius * 0.7f)
            ChangeMoveState(MechMoveStateId.ChasePlayer);
        else
            ChangeMoveState(MechMoveStateId.CirclePlayer);

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
            ChangeSuperState(MechSuperStateId.JumpSpecial);
        else if (_currentSpinCooldown <= 0 && roll < 0.20f)
            ChangeSuperState(MechSuperStateId.SpinSpecial);
    }

    // ===================== Projectiles =====================

    public void SpawnGunProjectile(bool fromLeft)
    {
        if (BulletScene == null) return;

        var barrel = fromLeft ? _leftBarrel : _rightBarrel;
        if (barrel == null) return;

        var bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);

        bullet.GlobalTransform = barrel.GlobalTransform;

        Vector3 dir = barrel.GlobalTransform.Basis.Y;
        bullet.LookAt(bullet.GlobalPosition + dir, Vector3.Up);

        bullet.Initialize(dir);
    }

    public void SpawnRocket(bool fromLeft)
    {
        if (RocketScene == null) return;

        // UWAGA: w twoim kodzie był bug: wybierałeś _leftBarrel zamiast launchera.
        var launcher = fromLeft ? _leftLauncher : _rightLauncher;
        if (launcher == null) return;

        var rocket = RocketScene.Instantiate<Rocket>();
        GetTree().CurrentScene.AddChild(rocket);

        rocket.GlobalTransform = launcher.GlobalTransform;

        Vector3 dir = launcher.GlobalTransform.Basis.Y;
        rocket.LookAt(rocket.GlobalPosition + dir, Vector3.Up);

        rocket.Initialize(dir);
    }

    // ===================== LookAt smoothing =====================

    private void HandleLookAtModifiers(double delta)
    {
        if (HeadLookAt == null || LeftArmLookAt == null || RightArmLookAt == null)
            return;

        float influence = HeadLookAt.Influence;
        float target = LookAtActive ? 1f : 0f;
        float lerped = Mathf.Lerp(influence, target, LookAtInfluenceSpeed * (float)delta);

        HeadLookAt.Influence = lerped;
        LeftArmLookAt.Influence = lerped;
        RightArmLookAt.Influence = lerped;
    }

    public void DisableLookAt()
    {
        LookAtActive = false;
        HeadLookAt.Influence = 0f;
        LeftArmLookAt.Influence = 0f;
        RightArmLookAt.Influence = 0f;
    }
}
