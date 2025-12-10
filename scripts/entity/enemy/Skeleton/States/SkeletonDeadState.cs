using System;
using Godot;

public class SkeletonDeadState : IState
{
    private Skeleton _owner;
    public SkeletonDeadState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Hitbox.Active = false;
        _owner.PlayAnimationRandomized("Skeleton_Die");

        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.QueueFree();
    }


    public void Exit()
    {
        
    }

    public void PhysicsUpdate(double delta)
    {
        
    }

    public void Update(double delta)
    {
        
    }
}