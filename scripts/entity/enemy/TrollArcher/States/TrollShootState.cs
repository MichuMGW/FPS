

using System;
using Godot;

public class TrollShootState : IState
{
    private TrollArcher _owner;
    public TrollShootState(TrollArcher owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        
        _owner.TrollAnimation.Play("Troll_Shoot");
        _owner.BowAnimation.Play("Troll_Shoot");

        _owner.ShootArrow();
        SubscribeEvents();   
    }

    private void SubscribeEvents()
    {
        _owner.TrollAnimation.AnimationFinished += OnAnimationFinished;
    }

    private void UnsubscribeEvents()
    {
        _owner.TrollAnimation.AnimationFinished -= OnAnimationFinished;
    }


    private void OnAnimationFinished(StringName animName)
    {
        if(animName == "Troll_Shoot")
        {
            float distanceToPlayer = _owner.GlobalPosition.DistanceTo(_owner.Player.GlobalPosition);

            if (distanceToPlayer <= _owner.ShootDistance)
            {
                _owner.ChangeState(TrollStateId.Draw);
            }
            else
            {
                _owner.ChangeState(TrollStateId.Chase);
            }
        }
    }

    public void Exit()
    {
        _owner.DisableSpineLookAtTarget();
        UnsubscribeEvents();
    }


    public void PhysicsUpdate(double delta){}

    public void Update(double delta){}
}