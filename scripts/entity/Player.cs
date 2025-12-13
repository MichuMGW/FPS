using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
    [Export] public PlayerStatsResource BaseStats;
    public PlayerStatsManager Stats { get; private set; }
    public PlayerSpellController Spells { get; private set; }
    [Export] public PlayerHealthComponent Health {get; set;} //DODAĆ DO SCENY
    public PlayerMovement Movement { get; private set; }
    // [Export] public SpellCastManager SpellCastManager {get; set;}
    [Export] public Node3D AimTarget {get; private set;}
    [Export] public KnockbackComponent Knockback { get; private set; }
    public AnimationTree AnimTree { get; private set; }

     // ---------- FSM: SUPER ----------
    public PlayerSuperStateId CurrentSuperStateId { get; private set; }
    private IState _currentSuperState;
    private Dictionary<PlayerSuperStateId, IState> _superStates;

    // ---------- FSM: MOVE ----------
    public PlayerMoveStateId CurrentMoveStateId { get; private set; }
    private IState _currentMoveState;
    private Dictionary<PlayerMoveStateId, IState> _moveStates;

    // ---------- FSM: PRIMARY ACTION ----------
    public PlayerPrimaryActionStateId CurrentPrimaryActionStateId { get; private set; }
    private IState _currentPrimaryActionState;
    private Dictionary<PlayerPrimaryActionStateId, IState> _primaryActionStates;

    // ---------- FSM: SECONDARY ACTION ----------
    public PlayerSecondaryActionStateId CurrentSecondaryActionStateId { get; private set; }
    private IState _currentSecondaryActionState;
    private Dictionary<PlayerSecondaryActionStateId, IState> _secondaryActionStates;

    public SpellSlot CurrentPrimaryCastingSlot { get; set; } = SpellSlot.LeftHand;
    public SpellSlot CurrentSecondaryCastingSlot { get; set; } = SpellSlot.RightHand;

    public float StunTimeLeft { get; set; }



    public override void _Ready()
    {
        FindNodes();

        Stats.InitializeFromResource(BaseStats);
        InitializeComponents();

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

    public override void _Process(double delta)
    {
        _currentSuperState?.Update(delta);

        if (CurrentSuperStateId == PlayerSuperStateId.Alive)
        {
            _currentMoveState?.Update(delta);
            _currentPrimaryActionState?.Update(delta);
            _currentSecondaryActionState?.Update(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentSuperState?.PhysicsUpdate(delta);

        if (CurrentSuperStateId == PlayerSuperStateId.Alive)
        {
            _currentMoveState?.PhysicsUpdate(delta);
            _currentPrimaryActionState?.PhysicsUpdate(delta);
            _currentSecondaryActionState?.PhysicsUpdate(delta);
        }

        MoveAndSlide();
    }

    private void FindNodes()
    {
        Stats = GetNode<PlayerStatsManager>("PlayerStatsManager");
        if(Stats == null)
        {
            GD.PrintErr("Player: PlaterStatsManager is null");
        }
        Health = GetNode<PlayerHealthComponent>("PlayerHealthComponent");
        Spells = GetNode<PlayerSpellController>("PlayerSpellController");
        Movement = GetNode<PlayerMovement>("PlayerMovement");
        AnimTree = GetNode<AnimationTree>("Head/Camera3D/Arms/AnimationTree");
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
        _currentSuperState.Enter();
    }

    public void ChangeMoveState(PlayerMoveStateId newState)
    {
        if (_currentMoveState != null && CurrentMoveStateId == newState) return;

        _currentMoveState?.Exit();
        CurrentMoveStateId = newState;
        _currentMoveState = _moveStates[newState];
        _currentMoveState.Enter();
    }

    public void ChangePrimaryActionState(PlayerPrimaryActionStateId newState)
    {
        if (_currentPrimaryActionState != null && CurrentPrimaryActionStateId == newState) return;

        _currentPrimaryActionState?.Exit();
        CurrentPrimaryActionStateId = newState;
        _currentPrimaryActionState = _primaryActionStates[newState];
        _currentPrimaryActionState.Enter();
    }

    public void ChangeSecondaryActionState(PlayerSecondaryActionStateId newState)
    {
        if (_currentSecondaryActionState != null && CurrentSecondaryActionStateId == newState) return;

        _currentSecondaryActionState?.Exit();
        CurrentSecondaryActionStateId = newState;
        _currentSecondaryActionState = _secondaryActionStates[newState];
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
}
