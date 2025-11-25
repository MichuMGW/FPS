using Godot;

public partial class SkeletonChaseState : IState
{
    private Skeleton _owner;
    public SkeletonChaseState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.PlayAnimationRandomized("Skeleton_Run", true);
        _owner.Pathfind.Active = true;
    }

    public void Exit()
    {
        _owner.Pathfind.Active = false;
    }

    public void PhysicsUpdate(double delta)
    {
        float distanceToPlayer = _owner.GlobalPosition.DistanceTo(_owner.Player.GlobalPosition);
        _owner.VelocityComp.RotateTowardsMovement((float)delta);

        if (distanceToPlayer <= _owner.AttackDistance)
        {
            _owner.ChangeState(SkeletonStateId.Attack);
            return;
        }
    }

    public void Update(double delta){}
}