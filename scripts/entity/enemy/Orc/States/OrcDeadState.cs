using System;
using Godot;

public class OrcDeadState : IState
{
    private readonly Orc _owner;

    public OrcDeadState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.Active = false;
        _owner.Animation.Play("Orc_Die", 0.3f);

        _owner.Animation.AnimationFinished += OnAnimationFinished;
        _owner.Hurtbox.Active = false;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.QueueFree();
    }


    public void Exit() { }

    public void PhysicsUpdate(double delta) { }

    public void Update(double delta) { }
}
