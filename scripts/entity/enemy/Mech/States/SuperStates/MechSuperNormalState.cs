using Godot;

public class MechSuperNormalState : IState
{
    private readonly Mech _owner;

    public MechSuperNormalState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }
}
