

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

        ShootArrow();
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

    private void InitializeArrow()
    {
        if (_owner.ArrowProjectileScene == null || 
            _owner.ArrowSpawnPoint == null)
            return;

        var arrow = _owner.ArrowProjectileScene.Instantiate<ArrowProjectile>();

        arrow.GlobalTransform = _owner.ArrowSpawnPoint.GlobalTransform;

        Vector3 from = _owner.ArrowSpawnPoint.GlobalPosition;
        Vector3 to = _owner.AimTarget.GlobalPosition;

        Vector3 dir = (to - from).Normalized();

        arrow.Velocity = dir * arrow.Speed;

        _owner.GetTree().CurrentScene.AddChild(arrow);
    }

    private void ShootArrow()
    {
        InitializeArrow();
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