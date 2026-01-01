using System.Collections.Generic;
using Godot;

public abstract partial class StateMachineEnemy<TStateId> : Enemy where TStateId : struct
{
    protected IState CurrentState;
    protected Dictionary<TStateId, IState> States = new();
    protected TStateId CurrentStateId;

    private IUpdateState _updateState;
    private IPhysicsUpdateState _physicsState;

    public void ChangeState(TStateId newId)
    {
        if (CurrentState != null && EqualityComparer<TStateId>.Default.Equals(CurrentStateId, newId))
            return;

        CurrentState?.Exit();
        CurrentStateId = newId;
        CurrentState = States[newId];

        _updateState = CurrentState as IUpdateState;
        _physicsState = CurrentState as IPhysicsUpdateState;

        CurrentState.Enter();
    }

    protected override void TickBrain(float dt)
    {
        _physicsState?.PhysicsUpdate(dt);
        _updateState?.Update(dt);
    }
}
