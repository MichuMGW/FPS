public class SkeletonSummonerDeadState : IState
{
    private SkeletonSummoner _owner;
    public SkeletonSummonerDeadState(SkeletonSummoner owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("SkeletonSummoner_Die", 0.3f);
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