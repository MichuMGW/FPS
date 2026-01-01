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
        _owner.EnableMovement(false);
        _owner.DisableLookAt();
        _owner.PlayLocomotion("Mech_Die");

        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "Mech_Die")
            _owner.QueueFree();
    }


    public void Exit() { }
}
