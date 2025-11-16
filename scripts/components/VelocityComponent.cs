using Godot;
using System;

public partial class VelocityComponent : Node
{
    [Export] public float MaxSpeed { get; set; } = 5.0f;
    [Export] public float Acceleration { get; set; } = 10.0f;
    [Export] public float Deceleration { get; set; } = 14.0f;
    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public float TerminalVelocity { get; set; } = -50f;
    [Export] public StatusComponent Status { get; set; } 

    public Vector3 CurrentVelocity { get; private set; } = Vector3.Zero;
    public Vector3 DesiredVelocity { get; private set; } = Vector3.Zero;
    private float verticalVelocity = 0f;

    private float _baseMaxSpeed;
    private float _baseAcceleration;
    private float _baseDeceleration;

    private float _slowMultiplier = 1.0f;

    private CharacterBody3D _body;
    public override void _Ready()
    {
        _body = GetOwner<CharacterBody3D>();

        _baseMaxSpeed = MaxSpeed;
        _baseAcceleration = Acceleration;
        _baseDeceleration = Deceleration;

        Status.SlowStarted += OnSlowStarted;
        Status.SlowEnded += OnSlowEnded;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (!DesiredVelocity.IsZeroApprox())
        {
            AccelerateTowards(DesiredVelocity, dt);
        }
        else
        {
            // brak inputu / brak celu -> hamujemy
            DecelerateToZero(dt);
        }
        
        ApplyGravity(dt);

        Vector3 finalVelocity = CurrentVelocity;
        finalVelocity.Y = verticalVelocity;

        _body.Velocity = finalVelocity;
        GD.Print(_body.Velocity);
        _body.MoveAndSlide();
    }

    private void ApplyGravity(float delta)
    {
        // spadanie tylko jeśli NIE jesteśmy na ziemi
        if (!_body.IsOnFloor())
        {
            verticalVelocity -= Gravity * delta;
            if (verticalVelocity < TerminalVelocity)
                verticalVelocity = TerminalVelocity;
        }
    }

    public void SetDesiredDirection(Vector3 direction)
    {
        if (direction.IsZeroApprox())
        {
            DesiredVelocity = Vector3.Zero;
            return;
        }

        direction.Y = 0; //ruch po ziemi
        direction = direction.Normalized();
        DesiredVelocity = direction * MaxSpeed;
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

    //TA FUNKCJA POWINNA BYĆ W INTERFEJSIE, NP. IStats
     private void RecalculateStats()
    {
        MaxSpeed = _baseMaxSpeed * _slowMultiplier;

        Acceleration = _baseAcceleration * _slowMultiplier;
        Deceleration = _baseDeceleration * _slowMultiplier;
    }

    //DO WERYFIKACJI
    private void OnSlowStarted(float slowAmount){
         slowAmount = Mathf.Clamp(slowAmount, 0f, 1f);
        _slowMultiplier = 1f - slowAmount;
        RecalculateStats();
    }

    private void OnSlowEnded(){
        GD.Print("SLOW ENDED");
        _slowMultiplier = 1f;
        RecalculateStats();
    }
}
