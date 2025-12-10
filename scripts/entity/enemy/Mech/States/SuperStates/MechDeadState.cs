using System;
using Godot;

public class MechDeadState : IState
{
    private readonly Mech _owner;

    public MechDeadState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.Active = false;
        _owner.VelocityComp.StopInstantly();
        _owner.PlayLocomotion("Mech_Die");

        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.QueueFree();
    }


    public void Exit() { }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }
}
