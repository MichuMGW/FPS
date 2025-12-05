using Godot;

public class MechMoveIdleState : IState
{
    private readonly Mech _owner;

    public MechMoveIdleState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
        _owner.PlayLocomotion("Mech_Idle");
    }

    public void Exit() { }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }
}
