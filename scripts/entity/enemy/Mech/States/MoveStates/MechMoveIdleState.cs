using Godot;

public class MechMoveIdleState : IState, IPhysicsUpdateState
{
    private readonly Mech _owner;

    public MechMoveIdleState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.EnableMovement(false);
        _owner.PlayLocomotion("Mech_Idle");
    }

    public void Exit() { }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }
}
