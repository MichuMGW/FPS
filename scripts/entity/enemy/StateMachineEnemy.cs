using System.Collections.Generic;

public abstract partial class StateMachineEnemy<TStateId> : Enemy
    where TStateId : struct
{
    protected IState CurrentState;
    protected Dictionary<TStateId, IState> States = new();
    protected TStateId CurrentStateId;

    public override void _Process(double delta) => CurrentState?.Update(delta);
    public override void _PhysicsProcess(double delta) => CurrentState?.PhysicsUpdate(delta);

    public void ChangeState(TStateId newId)
    {
        if (CurrentState != null && EqualityComparer<TStateId>.Default.Equals(CurrentStateId, newId))
            return;

        CurrentState?.Exit();
        CurrentStateId = newId;
        CurrentState = States[newId];
        CurrentState.Enter();
    }
}
