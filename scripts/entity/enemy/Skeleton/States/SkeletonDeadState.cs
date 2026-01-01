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
    public void Exit()
    {
        
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.QueueFree();
    }


}