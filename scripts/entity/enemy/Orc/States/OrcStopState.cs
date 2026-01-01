using System;
using Godot;


public class OrcStopState : IState, IPhysicsUpdateState
{
    private readonly Orc _owner;

    public OrcStopState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("Orc_RunToStop", 0.2, 0.6f);
        _owner.Animation.AnimationFinished += OnAnimationFinished;

        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "Orc_RunToStop")
        {
            _owner.ChangeState(OrcStateId.Chase);
        }
    }

    public void Exit()
    {
        _owner.Pathfind.SetPhysicsProcess(true);
        _owner.Animation.AnimationFinished -= OnAnimationFinished;
    }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.RotateTowardsMovement((float)delta);
    }
}
