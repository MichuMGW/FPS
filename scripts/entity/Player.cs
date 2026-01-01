using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
    [Export] public PlayerStatsResource BaseStats;
    public PlayerStatsManager Stats { get; private set; }
    public PlayerSpellController Spells { get; private set; }
    [Export] public PlayerHealthComponent Health { get; set; } //DODAĆ DO SCENY
    public PlayerMovement Movement { get; private set; }
    // [Export] public SpellCastManager SpellCastManager {get; set;}
    [Export] public Node3D AimTarget { get; set; }
    [Export] public KnockbackComponent Knockback { get; private set; }
    [Export] public Camera3D Camera { get; set; }
    [Export] public SimpleHurtboxComponent Hurtbox { get; set; }

    public AnimationPlayer Animation { get; private set; }
    public AnimationTree AnimTree { get; private set; }
    private AnimationNodeStateMachinePlayback _leftSmp;
    private AnimationNodeStateMachinePlayback _rightSmp;

    // ---------- FSM: SUPER ----------
    public PlayerSuperStateId CurrentSuperStateId { get; private set; }
    private IState _currentSuperState;
    private Dictionary<PlayerSuperStateId, IState> _superStates;
    private IUpdateState _currentSuperUpdate;
    private IPhysicsUpdateState _currentSuperPhysics;

    // ---------- FSM: MOVE ----------
    public PlayerMoveStateId CurrentMoveStateId { get; private set; }
    private IState _currentMoveState;
    private Dictionary<PlayerMoveStateId, IState> _moveStates;
    private IUpdateState _currentMoveUpdate;
    private IPhysicsUpdateState _currentMovePhysics;

    // ---------- FSM: PRIMARY ACTION ----------
    public PlayerPrimaryActionStateId CurrentPrimaryActionStateId { get; private set; }
    private IState _currentPrimaryActionState;
    private Dictionary<PlayerPrimaryActionStateId, IState> _primaryActionStates;
    private IUpdateState _currentPrimaryActionUpdate;
    private IPhysicsUpdateState _currentPrimaryActionPhysics;

    // ---------- FSM: SECONDARY ACTION ----------
    public PlayerSecondaryActionStateId CurrentSecondaryActionStateId { get; private set; }
    private IState _currentSecondaryActionState;
    private Dictionary<PlayerSecondaryActionStateId, IState> _secondaryActionStates;
    private IUpdateState _currentSecondaryActionUpdate;
    private IPhysicsUpdateState _currentSecondaryActionPhysics;

    public SpellSlot CurrentPrimaryCastingSlot { get; set; } = SpellSlot.LeftHand;
    public SpellSlot CurrentSecondaryCastingSlot { get; set; } = SpellSlot.RightHand;

    public float StunTimeLeft { get; set; }



    public override void _Ready()
    {
        FindNodes();

        Stats.InitializeFromResource(BaseStats);
        InitializeComponents();
        InitializeAnimationTree();

        InitializeSuperStates();
        InitializeMoveStates();
        InitializePrimaryActionStates();
        InitializeSecondaryActionStates();

        ChangeSuperState(PlayerSuperStateId.Alive);
        ChangeMoveState(PlayerMoveStateId.Airborne);
        ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
        ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);

        Health.EntityDied += OnDied;
    }

    private void InitializeComponents()
    {
        Movement.Initialize();
    }

    private void InitializeAnimationTree()
    {
        AnimTree.Active = true;

        _leftSmp = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/LeftHand/playback");
        _rightSmp = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/RightHand/playback");

        PlayLeftArmAnimation("L_Idle");
        PlayRightArmAnimation("R_Idle");
    }

    public void PlayLeftArmAnimation(string stateName, bool forceReset = false)
    {
        if (forceReset)
        {
            _leftSmp?.Start($"Arms_{stateName}", true);
        }
        else
        {
            _leftSmp?.Travel($"Arms_{stateName}");
        }
    }

    public void PlayRightArmAnimation(string stateName, bool forceReset = false)
    {
        if (forceReset)
        {
            _rightSmp?.Start($"Arms_{stateName}", true);
        }
        else
        {
            _rightSmp?.Travel($"Arms_{stateName}");
        }
    }

    public override void _Process(double delta)
    {
        _currentSuperUpdate?.Update(delta);

        if (CurrentSuperStateId == PlayerSuperStateId.Alive)
        {
            _currentMoveUpdate?.Update(delta);
            _currentPrimaryActionUpdate?.Update(delta);
            _currentSecondaryActionUpdate?.Update(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentSuperPhysics?.PhysicsUpdate(delta);

        if (CurrentSuperStateId == PlayerSuperStateId.Alive)
        {
            _currentMovePhysics?.PhysicsUpdate(delta);
            _currentPrimaryActionPhysics?.PhysicsUpdate(delta);
            _currentSecondaryActionPhysics?.PhysicsUpdate(delta);
        }

        MoveAndSlide();
    }

    private void FindNodes()
    {
        Stats = GetNode<PlayerStatsManager>("PlayerStatsManager");
        if (Stats == null)
        {
            GD.PrintErr("Player: PlaterStatsManager is null");
        }
        Health = GetNode<PlayerHealthComponent>("PlayerHealthComponent");
        Spells = GetNode<PlayerSpellController>("PlayerSpellController");
        Movement = GetNode<PlayerMovement>("PlayerMovement");

        AnimTree = GetNode<AnimationTree>("Head/Camera3D/Arms/AnimationTree");
        Animation = GetNode<AnimationPlayer>("Head/Camera3D/Arms/AnimationPlayer");

        Camera = GetNode<Camera3D>("Head/Camera3D");

        Hurtbox = GetNode<SimpleHurtboxComponent>("SimpleHurtboxComponent");
    }

    private void InitializeSuperStates()
    {
        _superStates = new()
        {
            { PlayerSuperStateId.Alive, new PlayerSuperAliveState(this) },
            { PlayerSuperStateId.Dead, new PlayerSuperDeadState(this) },
            { PlayerSuperStateId.Stunned, new PlayerSuperStunnedState(this) },
        };
    }

    private void InitializeMoveStates()
    {
        _moveStates = new()
        {
            { PlayerMoveStateId.Grounded, new PlayerMoveGroundedState(this) },
            { PlayerMoveStateId.Airborne, new PlayerMoveAirborneState(this) },
            { PlayerMoveStateId.Dash, new PlayerMoveDashState(this) }
        };
    }

    private void InitializePrimaryActionStates()
    {
        _primaryActionStates = new()
        {
            { PlayerPrimaryActionStateId.None, new PlayerPrimaryActionNoneState(this) },
            { PlayerPrimaryActionStateId.Casting, new PlayerPrimaryActionCastingState(this) }
        };
    }

    private void InitializeSecondaryActionStates()
    {
        _secondaryActionStates = new()
        {
            { PlayerSecondaryActionStateId.None, new PlayerSecondaryActionNoneState(this) },
            { PlayerSecondaryActionStateId.Casting, new PlayerSecondaryActionCastingState(this) }
        };
    }


    public void ChangeSuperState(PlayerSuperStateId newState)
    {
        if (_currentSuperState != null && CurrentSuperStateId == newState) return;

        _currentSuperState?.Exit();
        CurrentSuperStateId = newState;
        _currentSuperState = _superStates[newState];

        _currentSuperUpdate = _currentSuperState as IUpdateState;
        _currentSuperPhysics = _currentSuperState as IPhysicsUpdateState;

        _currentSuperState.Enter();
    }

    public void ChangeMoveState(PlayerMoveStateId newState)
    {
        if (_currentMoveState != null && CurrentMoveStateId == newState) return;

        _currentMoveState?.Exit();
        CurrentMoveStateId = newState;
        _currentMoveState = _moveStates[newState];

        _currentMoveUpdate = _currentMoveState as IUpdateState;
        _currentMovePhysics = _currentMoveState as IPhysicsUpdateState;

        _currentMoveState.Enter();
    }

    public void ChangePrimaryActionState(PlayerPrimaryActionStateId newState)
    {
        if (_currentPrimaryActionState != null && CurrentPrimaryActionStateId == newState) return;

        _currentPrimaryActionState?.Exit();
        CurrentPrimaryActionStateId = newState;
        _currentPrimaryActionState = _primaryActionStates[newState];

        _currentPrimaryActionUpdate = _currentPrimaryActionState as IUpdateState;
        _currentPrimaryActionPhysics = _currentPrimaryActionState as IPhysicsUpdateState;

        _currentPrimaryActionState.Enter();
    }

    public void ChangeSecondaryActionState(PlayerSecondaryActionStateId newState)
    {
        if (_currentSecondaryActionState != null && CurrentSecondaryActionStateId == newState) return;

        _currentSecondaryActionState?.Exit();
        CurrentSecondaryActionStateId = newState;
        _currentSecondaryActionState = _secondaryActionStates[newState];

        _currentSecondaryActionUpdate = _currentSecondaryActionState as IUpdateState;
        _currentSecondaryActionPhysics = _currentSecondaryActionState as IPhysicsUpdateState;

        _currentSecondaryActionState.Enter();
    }


    private void OnDied()
    {
        ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
        ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);

        ChangeSuperState(PlayerSuperStateId.Dead);
    }

    public void Stun(float seconds)
    {
        StunTimeLeft = Mathf.Max(StunTimeLeft, seconds);
        ChangeSuperState(PlayerSuperStateId.Stunned);
    }

    public void StartDash(DashSpellDefinition def, Vector3 direction)
    {
        if (_moveStates[PlayerMoveStateId.Dash] is PlayerMoveDashState dashState)
        {
            dashState.Setup(def, direction);
            ChangeMoveState(PlayerMoveStateId.Dash);
        }
    }

}