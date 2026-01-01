using Godot;

public class PlayerMoveGroundedState : IState, IPhysicsUpdateState
{
    private readonly Player _player;

    public PlayerMoveGroundedState(Player player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.Movement.ResetJumps();
        // opcjonalnie: animacja "idle/run"
    }

    public void Exit() { }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;

        // jeśli nie jesteś na ziemi -> airborne (spadek)
        if (!_player.IsOnFloor())
        {
            _player.ChangeMoveState(PlayerMoveStateId.Airborne);
            return;
        }

        Vector3 moveDir = _player.Movement.ReadMoveInput();
        float speed = _player.Movement.Speed;

        _player.Movement.ApplyGroundMove(moveDir, speed);

        // Jump
        if (_player.Movement.TryJump())
        {
            _player.ChangeMoveState(PlayerMoveStateId.Airborne);
            return;
        }

        // grawitacja/utrzymanie Y=0 na ziemi
        _player.Movement.ApplyGravity(dt);
    }
}
