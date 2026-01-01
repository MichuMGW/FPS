using System;
using Godot;

public class SkeletonSummonerDeadState : IState
{
    private SkeletonSummoner _owner;
    public SkeletonSummonerDeadState(SkeletonSummoner owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("SkeletonSummoner_Die", 0.3f);
        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.QueueFree();
    }

    public void Exit()
    {
        
    }
}