public class SkeletonSummonerIdleState : IState
{
    private SkeletonSummoner _owner;
    private float _idleTime;
    public SkeletonSummonerIdleState(SkeletonSummoner owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Animation.Play("SkeletonSummoner_Idle", 0.3f);
        _idleTime = 0f;
    }

    public void Exit()
    {
        
    }

    public void PhysicsUpdate(double delta)
    {
        
    }

    public void Update(double delta)
    {
        var dt = (float)delta;

        _idleTime += dt;
        if(_idleTime > _owner.SummonCooldown)
        {
            _owner.ChangeState(SkeletonSummonerStateId.Summon);
        }
    }
}