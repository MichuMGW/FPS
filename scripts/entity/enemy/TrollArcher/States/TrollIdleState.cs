

public class TrollIdleState : IState
{
    private TrollArcher _owner;
    public TrollIdleState(TrollArcher owner)
    {
        _owner = owner;
    }

    public void Enter(){}

    public void Exit(){}

    public void PhysicsUpdate(double delta){}

    public void Update(double delta){}
}