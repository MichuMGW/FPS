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
        _owner.Animation.Play("Orc_Die");

        if (_owner.Hurtbox != null)
        {
            // najprostsze wyłączenie – skoro nie masz flagi Enabled:
            foreach (var hb in _owner.Hurtbox.hurtboxes)
            {
                hb.SetDeferred("monitoring", false);
                hb.SetDeferred("monitorable", false);
            }
        }

        // tu animacja śmierci
        // _owner.Animation.Play("Death");

        // po animacji możesz zawołać QueueFree() (np. z AnimationFinished)
    }

    public void Exit() { }

    public void PhysicsUpdate(double delta) { }

    public void Update(double delta) { }
}
