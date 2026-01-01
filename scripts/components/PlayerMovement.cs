using Godot;

public partial class PlayerMovement : Node
{
    private Player _player; // zamiast CharacterBody3D, bo i tak kontekst
    [Export] public float SprintMultiplier = 2f;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float AirControl = 0.25f; // ile kontroli w powietrzu (0..1)

    public float Speed { get; private set; }
    private float _jumpForce;
    private int _maxJumpCount;


    public int JumpsLeft { get; private set; }

    private PlayerStatsManager _stats;

    public override void _Ready()
    {
        _player = GetOwner<Player>();
    }

    public void Initialize()
    {
        _stats = _player.Stats;
        _stats.StatChanged += OnStatChanged;

        PullStats();
        ResetJumps();
    }

    public override void _ExitTree()
    {
        if (_stats != null)
            _stats.StatChanged -= OnStatChanged;
    }

    private void PullStats()
    {
        Speed = _stats.GetStat(StatId.MoveSpeed);
        _jumpForce = _stats.GetStat(StatId.JumpForce);
        _maxJumpCount = Mathf.Max(1, Mathf.RoundToInt(_stats.GetStat(StatId.JumpCount)));
    }

    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        switch ((StatId)statId)
        {
            case StatId.MoveSpeed:
            case StatId.JumpForce:
            case StatId.JumpCount:
                PullStats();
                // jeśli jump count się zmienił, a jesteś w powietrzu, nie rób cudów:
                JumpsLeft = Mathf.Min(JumpsLeft, _maxJumpCount);
                break;
        }
    }

    public void ResetJumps()
    {
        JumpsLeft = _maxJumpCount;
    }

    public Vector3 ReadMoveInput()
    {
        Vector3 dir = Vector3.Zero;

        if (Input.IsActionPressed("MoveForward"))
            dir += -_player.GlobalTransform.Basis.Z;
        if (Input.IsActionPressed("MoveLeft"))
            dir += -_player.GlobalTransform.Basis.X;
        if (Input.IsActionPressed("MoveRight"))
            dir += _player.GlobalTransform.Basis.X;
        if (Input.IsActionPressed("MoveBackward"))
            dir += _player.GlobalTransform.Basis.Z;

        return dir.Normalized();
    }

    public void ApplyGravity(float dt)
    {
        if (!_player.IsOnFloor())
            _player.Velocity += _player.GetGravity() * dt;
        else
            _player.Velocity = new Vector3(_player.Velocity.X, 0f, _player.Velocity.Z);
    }

    public void ApplyGroundMove(Vector3 moveDir, float speed)
    {
        _player.Velocity = new Vector3(moveDir.X * speed, _player.Velocity.Y, moveDir.Z * speed);
    }

    public void ApplyAirMove(Vector3 moveDir, float speed, float dt)
    {
        // w powietrzu nie setujemy XZ na sztywno, tylko blendujemy
        Vector3 current = new Vector3(_player.Velocity.X, 0f, _player.Velocity.Z);
        Vector3 target  = new Vector3(moveDir.X * speed, 0f, moveDir.Z * speed);

        Vector3 blended = current.Lerp(target, AirControl * dt * 60f); // *60 żeby było stabilne
        _player.Velocity = new Vector3(blended.X, _player.Velocity.Y, blended.Z);
    }

    public void ApplyDashMove(Vector3 dir, float dashSpeed, bool keepY = true)
    {
        float y = keepY ? _player.Velocity.Y : 0f;
        _player.Velocity = new Vector3(dir.X * dashSpeed, y, dir.Z * dashSpeed);
    }

    public bool TryJump()
    {
        if (!Input.IsActionJustPressed("Jump"))
            return false;

        if (JumpsLeft <= 0)
            return false;

        JumpsLeft--;
        _player.Velocity = new Vector3(_player.Velocity.X, _jumpForce, _player.Velocity.Z);
        return true;
    }

    public Vector3 GetDashDirection()
    {
        Vector3 dir = ReadMoveInput();
        dir.Y = 0f;

        if (dir.LengthSquared() < 0.0001f)
        {
            dir = -_player.GlobalTransform.Basis.Z;
            dir.Y = 0f;
        }

        return dir.Normalized();
    }

    public float EvaluateDashSpeed(float dashForce, float t01)
    {
        t01 = Mathf.Clamp(t01, 0f, 1f);

        // ease-out: mocny start, szybciej gaśnie na końcu
        float factor = Mathf.Pow(1f - t01, 1.5f);
        factor *= factor; // (1 - t)^2

        return dashForce * factor;
    }
}
