using Godot;
using System;

public partial class VelocityComponent : Node
{
    // ===== Debug toggles =====
    [Export] public bool DebugLogStatWrites { get; set; } = true;
    [Export] public bool DebugIncludeStackTrace { get; set; } = true;
    private float _maxSpeed = 5.0f;
    [Export]
    public float MaxSpeed
    {
        get => _maxSpeed;
        set => _maxSpeed = value;
    }

    private float _acceleration = 10.0f;
    [Export]
    public float Acceleration
    {
        get => _acceleration;
        set => _acceleration = value;
    }

    private float _deceleration = 14.0f;
    [Export]
    public float Deceleration
    {
        get => _deceleration;
        set => _deceleration = value;
    }

    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public float RotationSpeed { get; set; } = 8f;
    [Export] public float TerminalVelocity { get; set; } = -50f;

    public bool Active { get; set; } = true;

    public Vector3 CurrentVelocity { get; private set; } = Vector3.Zero;
    public Vector3 DesiredVelocity { get; private set; } = Vector3.Zero;
    private float verticalVelocity = 0f;

    // ===== Base snapshot =====
    private float _baseMaxSpeed;
    private float _baseAcceleration;
    private float _baseDeceleration;

    private float _slowMultiplier = 1.0f;
    private Vector3 _desiredDir = Vector3.Zero;

    private CharacterBody3D _body;

    public override void _Ready()
    {
        _body = GetOwner<CharacterBody3D>();
        SetBaseStats();
    }

    public void Tick(float dt)
    {
        if (!Active) return;

        DesiredVelocity = _desiredDir.IsZeroApprox()
            ? Vector3.Zero
            : _desiredDir * MaxSpeed;

        if (!DesiredVelocity.IsZeroApprox())
            AccelerateTowards(DesiredVelocity, dt);
        else
            DecelerateToZero(dt);

        ApplyGravity(dt);

        Vector3 finalVelocity = CurrentVelocity;
        finalVelocity.Y = verticalVelocity;

        _body.Velocity = finalVelocity;
        _body.MoveAndSlide();
    }

    private void ApplyGravity(float delta)
    {
        if (!_body.IsOnFloor())
        {
            verticalVelocity -= Gravity * delta;
            if (verticalVelocity < TerminalVelocity)
                verticalVelocity = TerminalVelocity;
        }
    }

    public void RotateTowardsMovement(float delta)
    {
        Vector3 vel = CurrentVelocity;
        vel.Y = 0;

        if (vel.LengthSquared() < 0.001f)
            return;

        Vector3 desiredDir = vel.Normalized();
        Vector3 currentForward = _body.GlobalTransform.Basis.Z;
        Vector3 newForward = currentForward.Slerp(desiredDir, RotationSpeed * delta).Normalized();
        Vector3 targetPos = _body.GlobalPosition - newForward;

        _body.LookAt(targetPos, Vector3.Up);
    }

    public void SetDesiredDirection(Vector3 direction)
    {
        if (direction.IsZeroApprox())
        {
            _desiredDir = Vector3.Zero;
            return;
        }

        direction.Y = 0;
        _desiredDir = direction.Normalized();
    }

    public void AccelerateTowards(Vector3 targetVelocity, float delta)
    {
        float t = 1.0f - Mathf.Exp(-Acceleration * delta);
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        CurrentVelocity = CurrentVelocity.Lerp(targetVelocity, t);
    }

    public void DecelerateToZero(float delta)
    {
        if (CurrentVelocity.IsZeroApprox())
            return;

        float t = 1.0f - Mathf.Exp(-Deceleration * delta);
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        CurrentVelocity = CurrentVelocity.Lerp(Vector3.Zero, t);
    }

    public void StopInstantly()
    {
        DesiredVelocity = Vector3.Zero;
        CurrentVelocity = Vector3.Zero;
        if (_body != null)
            _body.Velocity = Vector3.Zero;
    }
    public void SetBaseStats()
    {
        _baseMaxSpeed = MaxSpeed;
        _baseAcceleration = Acceleration;
        _baseDeceleration = Deceleration;
    }

    private void RecalculateStats()
    {
        MaxSpeed = _baseMaxSpeed * _slowMultiplier;
        Acceleration = _baseAcceleration * _slowMultiplier;
        Deceleration = _baseDeceleration * _slowMultiplier;
    }

    public void SetMoveSpeedMultiplier(float mult)
    {
        _slowMultiplier = mult;
        RecalculateStats();
    }
}
