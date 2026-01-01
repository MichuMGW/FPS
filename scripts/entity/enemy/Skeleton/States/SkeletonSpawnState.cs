using System;
using Godot;

public class SkeletonSpawnState : IState
{
    private Skeleton _owner;
    public SkeletonSpawnState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.PlayAnimationRandomized("Skeleton_Spawn");
        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }
    public void Exit()
    {
        
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.ChangeState(SkeletonStateId.Chase);
        _owner.Animation.AnimationFinished -= OnAnimationFinished;
    }

}