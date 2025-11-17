

using System;
using Godot;

public class TrollDrawState : IState
{
    private TrollArcher _owner;
    private bool _bodyLookAtTarget;
    private bool _spineLookAtTarget;

    private Timer _bodyLookAtTimer;
    private Timer _drawEndTimer;
    private Timer _aimLockTimer;
    public TrollDrawState(TrollArcher owner)
    {
        _owner = owner;
    }
    public void Enter()
    {
        _owner.EnableSpineLookAtTarget();
        _owner.SetSpineLookAtPlayer();

        _bodyLookAtTarget = true;
        _spineLookAtTarget = true;

        InitializeBodyLookAtTimer();
        InitializeDrawEndTimer();
        InitializeAimLockTimer();

        _bodyLookAtTimer.Start();

        _owner.TrollAnimation.Play("Troll_Draw");
        _owner.BowAnimation.Play("Troll_Draw");
        _owner.ArrowAnimation.Play("Troll_Draw");

        _owner.TrollAnimation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        if(animName == "Troll_Draw")
        {
            // _owner.DisableSpineLookAtTarget();
            _drawEndTimer.Start();
            _bodyLookAtTarget = false;
            // PrepareToShoot();
            // _owner.ChangeState(TrollStateId.Shoot);
        }
    }

    private void InitializeBodyLookAtTimer()
    {
        _bodyLookAtTimer = new Timer();
        _bodyLookAtTimer.OneShot = true;
        _bodyLookAtTimer.WaitTime = 1f;
        _bodyLookAtTimer.Timeout += () =>
        {
            _bodyLookAtTarget = false;
            _owner.SetSpineLookAtAimTarget();
            
        };
        _owner.AddChild(_bodyLookAtTimer);
        
    }

    private void InitializeDrawEndTimer()
    {
        _drawEndTimer = new Timer();
        _drawEndTimer.OneShot = true;
        _drawEndTimer.WaitTime = _owner.AimTime;
        _drawEndTimer.Timeout += () =>
        {
            _aimLockTimer.Start();
        };
        _owner.AddChild(_drawEndTimer);
    }

    private void InitializeAimLockTimer()
    {
        _aimLockTimer = new Timer();
        _aimLockTimer.OneShot = true;
        _aimLockTimer.WaitTime = 0.2f;
        _aimLockTimer.Timeout += () =>
        {
            _owner.ChangeState(TrollStateId.Shoot);
        };
        _owner.AddChild(_aimLockTimer);
    }

    public void Exit()
    {
        _bodyLookAtTimer.QueueFree();
        _drawEndTimer.QueueFree();
        _aimLockTimer.QueueFree();
        _owner.TrollAnimation.AnimationFinished -= OnAnimationFinished;
        // throw new System.NotImplementedException();
    }

    public void PhysicsUpdate(double delta)
    {
        if (_bodyLookAtTarget)
        {
            _owner.RotateHorizontallyTowardsPlayer((float)delta);
        }
        _owner.UpdateSpineLookTarget((float)delta);
    }

    public void Update(double delta)
    {

    }

}