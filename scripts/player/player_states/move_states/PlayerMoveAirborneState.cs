using Godot;

public class PlayerMoveAirborneState : IState, IPhysicsUpdateState
{
    private readonly Player _player;

    // możesz to wynieść do statów / configu
    private const float KnockbackAirControl = 0.05f;

    public PlayerMoveAirborneState(Player player)
    {
        _player = player;
    }

    public void Enter()
    {
        // nic - jumpy resetuje Grounded
    }

    public void Exit()
    {
    }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;

        // jeśli wylądował -> grounded
        if (_player.IsOnFloor())
        {
            _player.ChangeMoveState(PlayerMoveStateId.Grounded);
            return;
        }

        // 1) Najpierw knockback, bo ma priorytet nad inputem
        if (_player.Knockback != null)
        {
            _player.Knockback.PhysicsUpdate(delta);
        }

        // 2) Ruch w powietrzu z ograniczoną kontrolą
        Vector3 moveDirection = _player.Movement.ReadMoveInput();
        float speed = _player.Movement.Speed;

        float airControl = _player.Movement.AirControl;
        if (_player.Knockback != null && _player.Knockback.IsActive)
        {
            airControl = KnockbackAirControl;
        }

        ApplyAirMoveWithControl(moveDirection, speed, dt, airControl);

        // 3) Skok w powietrzu (double/triple)
        // Jeśli chcesz blokować jump podczas knockbacku: dodaj warunek !player.Knockback.IsActive
        _player.Movement.TryJump();

        // 4) Grawitacja na końcu
        _player.Movement.ApplyGravity(dt);
    }

    private void ApplyAirMoveWithControl(Vector3 moveDirection, float speed, float dt, float airControl)
    {
        Vector3 currentHorizontal = new Vector3(_player.Velocity.X, 0f, _player.Velocity.Z);
        Vector3 targetHorizontal = new Vector3(moveDirection.X * speed, 0f, moveDirection.Z * speed);

        // stabilizacja względem FPS
        float t = airControl * dt * 60f;

        Vector3 blended = currentHorizontal.Lerp(targetHorizontal, t);
        _player.Velocity = new Vector3(blended.X, _player.Velocity.Y, blended.Z);
    }
}
