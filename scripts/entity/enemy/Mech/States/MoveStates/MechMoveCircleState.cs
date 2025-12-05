using Godot;

public class MechMoveCircleState : IState
{
    private readonly Mech _owner;
    private float _angle;

    public MechMoveCircleState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.LookAtActive = true;
        _owner.Pathfind.Active = false;
        _owner.Pathfind.SetPhysicsProcess(false);
        _owner.VelocityComp.MaxSpeed = _owner.MoveSpeed;
        _owner.PlayLocomotion("Mech_Walk");

        if (_owner.Player != null)
        {
            Vector3 toMech = _owner.GlobalPosition - _owner.Player.GlobalPosition;
            toMech.Y = 0;
            if (!toMech.IsZeroApprox())
                _angle = Mathf.Atan2(toMech.X, toMech.Z);
        }
    }

    public void Exit()
    {
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
        _owner.Pathfind.SetPhysicsProcess(true);
    }

    public void Update(double delta)
    {
        if (_owner.Player == null) return;

        _angle += _owner.CircleAngularSpeed * (float)delta;

        var center = _owner.Player.GlobalPosition;
        float r = _owner.CircleRadius;

        var target = center + new Vector3(
            Mathf.Sin(_angle) * r,
            0,
            Mathf.Cos(_angle) * r
        );

        var dir = target - _owner.GlobalPosition;
        dir.Y = 0;

        _owner.VelocityComp.SetDesiredDirection(dir);
    }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.RotateTowardsMovement((float)delta);
    }
}
