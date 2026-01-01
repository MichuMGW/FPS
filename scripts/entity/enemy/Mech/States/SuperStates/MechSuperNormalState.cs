using Godot;

public class MechSuperNormalState : IState
{
    private readonly Mech _owner;

    public MechSuperNormalState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.EnableMovement(true);
    }

    public void Exit() { }
}
