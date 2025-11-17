public interface IState
{
    public void Enter();
    public void Exit();
    public void Update(double delta);
    public void PhysicsUpdate(double delta);
}