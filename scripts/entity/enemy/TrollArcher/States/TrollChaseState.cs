using Godot;

public partial class TrollChaseState : IState
{
    private TrollArcher _owner;
    public TrollChaseState(TrollArcher owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Pathfind.Active = true;
        _owner.Pathfind.SetPlayerAsTarget();
        _owner.TrollAnimation.Play("Troll_Walk", 0.5);
    }

    public void Exit()
    {
        _owner.Pathfind.Active = false;
    }

    public void PhysicsUpdate(double delta)
    {
        float distanceToPlayer = _owner.GlobalPosition.DistanceTo(_owner.Player.GlobalPosition);
        _owner.VelocityComp.RotateTowardsMovement((float)delta);

        // Jeśli gracz jest wystarczająco blisko – przechodzimy do stanu strzelania
        if (distanceToPlayer <= _owner.ShootDistance)
        {
            _owner.ChangeState(TrollStateId.Draw);
            return;
        }

    }

    public void Update(double delta){}
}