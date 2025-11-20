using System;
using Godot;

public class OrcStopState : IState
{
    private readonly Orc _owner;
    private float _elapsed;

    // żeby nie stał w nieskończoność, jak coś się zjebało
    private const float MaxStopTime = 1.5f;

    public OrcStopState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _elapsed = 0f;
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
        _elapsed += (float)delta;

        // opcjonalnie obrót w stronę gracza przy hamowaniu
        _owner.VelocityComp.RotateTowardsMovement((float)delta);

        if (_owner.VelocityComp.CurrentVelocity.Length() <= 0.1f || _elapsed >= MaxStopTime)
        {
            // // wracamy do gonitwy
            // if (_owner.CurrentStateId != OrcStateId.Dead)
            //     _owner.ChangeState(OrcStateId.Chase);
        }
    }

    public void Update(double delta) { }
}
