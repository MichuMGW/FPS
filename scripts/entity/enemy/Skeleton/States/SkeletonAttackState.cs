using System;
using Godot;

public partial class SkeletonAttackState : IState
{
    private Skeleton _owner;
    public SkeletonAttackState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("Skeleton_Attack",0.2f);

        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.ChangeState(SkeletonStateId.Chase);
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