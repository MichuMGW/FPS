using System.Collections.Generic;

public sealed class StateSlot<TId> where TId : struct
{
    public TId CurrentId { get; private set; }
    public IState Current { get; private set; }

    private IUpdateState _update;
    private IPhysicsUpdateState _physics;

    private readonly Dictionary<TId, IState> _states;

    public StateSlot(Dictionary<TId, IState> states)
    {
        _states = states;
    }

    public void Change(TId id)
    {
        if (Current != null && EqualityComparer<TId>.Default.Equals(CurrentId, id))
            return;

        Current?.Exit();

        CurrentId = id;
        Current = _states[id];

        _update = Current as IUpdateState;
        _physics = Current as IPhysicsUpdateState;

        Current.Enter();
    }

    public void TickUpdate(double dt) => _update?.Update(dt);
    public void TickPhysics(double dt) => _physics?.PhysicsUpdate(dt);
}
