using Godot;

public class MechMoveChaseState : IState, IPhysicsUpdateState
{
    private readonly Mech _owner;

    public MechMoveChaseState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.LookAtActive = true;
        _owner.Pathfind.Active = true;
        _owner.Pathfind.SetPlayerAsTarget();
        _owner.VelocityComp.MaxSpeed = _owner.MoveSpeed;
        _owner.PlayLocomotion("Mech_Walk");
    }

    public void Exit()
    {
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.RotateTowardsMovement((float)delta);
    }
}
