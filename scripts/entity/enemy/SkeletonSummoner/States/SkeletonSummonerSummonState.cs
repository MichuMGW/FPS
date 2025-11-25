using System;
using Godot;

public class SkeletonSummonerSummonState : IState
{
    private SkeletonSummoner _owner;
    public SkeletonSummonerSummonState(SkeletonSummoner owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("SkeletonSummoner_Summon", 0.3f);
        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.ChangeState(SkeletonSummonerStateId.Idle);
    }

    public void Exit()
    {
        _owner.Animation.AnimationFinished -= OnAnimationFinished;
    }

    public void PhysicsUpdate(double delta)
    {
        
    }

    public void Update(double delta)
    {
        
    }
}