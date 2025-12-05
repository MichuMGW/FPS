using Godot;

public class MechAttackNoneState : IState
{
    private readonly Mech _owner;

    public MechAttackNoneState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }
}
