using Godot;

public class PlayerMoveDashState : IState, IPhysicsUpdateState
{
    private readonly Player _player;

    private DashSpellDefinition _def;
    private Vector3 _dir;

    private float _duration;
    private float _timeLeft;

    private float _baseSpeed;
    private float _dashStartSpeed;

    private const float FovStartOffset = 15f;
    private const float FovEndOffset = -15f;
    private const float FovLerpSpeed = 18f;

    // zapamiętane rzeczy do rollbacku
    private float _oldFov;

    private bool _fovChanged;

    public PlayerMoveDashState(Player player) => _player = player;

    public void Setup(DashSpellDefinition def, Vector3 direction)
    {
        _def = def;
        _dir = direction;
        _dir.Y = 0f;
        if (_dir.LengthSquared() > 0.0001f)
            _dir = _dir.Normalized();
    }

    public void Enter()
    {
        _duration = Mathf.Max(0.01f, _def.DashDuration);
        _timeLeft = _duration;

        _baseSpeed = _player.Movement.Speed;
        _dashStartSpeed = _baseSpeed * _def.SpeedMultiplier;

        _dir = _player.Movement.ReadMoveInput();
        _dir.Y = 0f;
        if (_dir.LengthSquared() < 0.0001f)
        {
            _dir = -_player.GlobalTransform.Basis.Z;
            _dir.Y = 0f;
        }
        _dir = _dir.Normalized();

        _player.CollisionMask &= ~PhysicsLayers.ENEMY_BODY;
        _player.Hurtbox.Active = false;

        var cam = _player.Camera;
        _fovChanged = cam != null;
        if (_fovChanged)
        {
            _oldFov = cam.Fov;
        }
    }

    public void Exit()
    {
        _player.CollisionMask |= PhysicsLayers.ENEMY_BODY;
        _player.Hurtbox.Active = true;

        if (_fovChanged && _player.Camera != null)
            _player.Camera.Fov = Mathf.Clamp(_oldFov, 1f, 179f);
    }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;
        _timeLeft -= dt;

        float t01 = 1f - (_timeLeft / _duration);
        t01 = Mathf.Clamp(t01, 0f, 1f);

        if (_fovChanged)
        {
            var cam = _player.Camera;

            float targetOffset = Mathf.Lerp(FovStartOffset, 0, t01);
            float targetFov = _oldFov + targetOffset;

            float k = 1f - Mathf.Exp(-FovLerpSpeed * dt);
            cam.Fov = Mathf.Lerp(cam.Fov, targetFov, k);

            cam.Fov = Mathf.Clamp(cam.Fov, 1f, 179f);
        }

        float alpha = 1f - Mathf.Pow(1f - t01, 2f);

        float speedNow = Mathf.Lerp(_dashStartSpeed, _baseSpeed, alpha);

        Vector3 inputDir = _player.Movement.ReadMoveInput();
        inputDir.Y = 0f;
        if (inputDir.LengthSquared() > 0.0001f)
        {
            float turn = Mathf.Clamp(10f * dt, 0f, 1f);
            _dir = _dir.Lerp(inputDir.Normalized(), turn).Normalized();
        }

        float y = _player.Velocity.Y;
        _player.Velocity = new Vector3(_dir.X * speedNow, y, _dir.Z * speedNow);

        if (_timeLeft <= 0f)
            _player.ChangeMoveState(_player.IsOnFloor() ? PlayerMoveStateId.Grounded : PlayerMoveStateId.Airborne);
    }

}
