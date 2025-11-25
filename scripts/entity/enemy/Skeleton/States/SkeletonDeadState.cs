public class SkeletonDeadState : IState
{
    private Skeleton _owner;
    public SkeletonDeadState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.PlayAnimationRandomized("Skeleton_Die");
    }

    public void Exit()
    {
        
    }

    public void PhysicsUpdate(double delta)
    {
        
    }

    public void Update(double delta)
    {
        
    }
}