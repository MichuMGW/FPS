public class OrcDeadState : IState
{
    private readonly Orc _owner;

    public OrcDeadState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.Active = false;
        _owner.Animation.Play("Orc_Die", 0.3f);

        _owner.Hurtbox.Active = false;

        //TODO: Usunięcie przeciwnika ze sceny po spełnieniu warunku na opuszczenie pola widzenia kamery
    }

    public void Exit() { }

    public void PhysicsUpdate(double delta) { }

    public void Update(double delta) { }
}
