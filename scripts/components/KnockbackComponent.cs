using Godot;

public partial class KnockbackComponent : Node
{
    [Export] public CharacterBody3D Body { get; set; }
    [Export] public PlayerStatsManager Stats { get; set; }

    [Export] public float Damping { get; set; } = 8f;
    [Export] public float ActiveThreshold { get; set; } = 0.05f;

    private Vector3 _knockbackVelocity = Vector3.Zero;

    public bool IsActive
    {
        get { return _knockbackVelocity.Length() > ActiveThreshold; }
    }

    public void ApplyKnockback(Vector3 impulse)
    {
        if (Stats == null)
        {
            _knockbackVelocity = impulse;
            return;
        }

        float resistance = Mathf.Clamp(
            Stats.GetStat(StatId.KnockbackResistance),
            0f, 1f
        );

        // pełna odporność → brak knockbacku
        if (resistance >= 1f)
            return;

        float multiplier = Stats.GetStat(StatId.KnockbackMultiplier);
        if (multiplier <= 0f)
            return;

        Vector3 finalImpulse = impulse * multiplier * (1f - resistance);
        _knockbackVelocity = finalImpulse;
    }

    public void PhysicsUpdate(double delta)
    {
        if (Body == null)
            return;

        Body.Velocity += _knockbackVelocity;

        _knockbackVelocity = _knockbackVelocity.Lerp(
            Vector3.Zero,
            (float)(Damping * delta)
        );
    }
}
