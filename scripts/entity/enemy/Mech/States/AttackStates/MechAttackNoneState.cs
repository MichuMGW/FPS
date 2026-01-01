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
}
