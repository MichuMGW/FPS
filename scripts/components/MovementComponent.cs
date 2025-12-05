using Godot;
using System;

public partial class MovementComponent : Node
{
    [Export] public float MaxSpeed { get; set; } = 5.0f;
    [Export] public float Acceleration { get; set; } = 10.0f;
    [Export] public float Deceleration { get; set; } = 14.0f;
    [Export] public StatusComponent Status { get; set; } 

    public Vector3 CurrentVelocity { get; private set; } = Vector3.Zero;
    public Vector3 DesiredVelocity { get; private set; } = Vector3.Zero;

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
            DecelerateToZero(dt);
        }

        _body.Velocity = CurrentVelocity;
        _body.MoveAndSlide();
    }

     public void SetDesiredDirection(Vector3 direction)
    {
        if (direction.IsZeroApprox())
        {
            DesiredVelocity = Vector3.Zero;
            return;
        }

        direction.Y = 0;
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

    //Rozważyć implementacje przy użyciu interfejsu IStats
     private void RecalculateStats()
    {
        MaxSpeed = _baseMaxSpeed * _slowMultiplier;

        Acceleration = _baseAcceleration * _slowMultiplier;
        Deceleration = _baseDeceleration * _slowMultiplier;
    }

    //TODO: Zweryfikować czy działa poprawnie po dodaniu wielu efektów spowolnienia
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
