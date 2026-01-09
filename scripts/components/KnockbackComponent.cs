using Godot;

public partial class KnockbackComponent : Node
{
    [Export] public CharacterBody3D Body { get; set; }
    [Export] public PlayerStatsManager Stats { get; set; }

    [Export] public float Damping { get; set; } = 12f;          // większe = szybciej wyhamuje
    [Export] public float ActiveThreshold { get; set; } = 0.05f;

    // limit żeby nie było “teleportu” przy dużych wartościach
    [Export] public float MaxHorizontalSpeed { get; set; } = 10f;

    private Vector3 _knockbackVelocity = Vector3.Zero;
    private bool _appliedOnce = false;

    public bool IsActive => _knockbackVelocity.Length() > ActiveThreshold;

    public void ApplyKnockback(Vector3 impulse)
    {
        if (Stats != null)
        {
            float resistance = Mathf.Clamp(Stats.GetStat(StatId.KnockbackResistance), 0f, 1f);
            if (resistance >= 1f) return;

            float multiplier = Stats.GetStat(StatId.KnockbackMultiplier);
            if (multiplier <= 0f) return;

            impulse *= multiplier * (1f - resistance);
        }

        _knockbackVelocity = impulse;
        _appliedOnce = false; // ważne: nowy knockback -> nowa aplikacja
    }

    public void ClearKnockback()
    {
        _knockbackVelocity = Vector3.Zero;
        _appliedOnce = false;
    }

    public void PhysicsUpdate(double delta)
    {
        if (Body == null) return;
        float dt = (float)delta;

        // 1. Jeśli to pierwsze klatka impulsu
        if (!_appliedOnce && IsActive)
        {
            // Nadpisujemy horyzontalną prędkość dla lepszego "czucia" uderzenia
            // Zamiast dodawać (+=), lepiej ustawić lub mocno zblendować, 
            // żeby pęd gracza nie sumował się dziwnie z odrzutem.
            Vector3 currentVel = Body.Velocity;
            Vector3 horizontalImpulse = new Vector3(_knockbackVelocity.X, 0, _knockbackVelocity.Z);

            // Ograniczamy siłę, by nie "teleportować" gracza
            if (horizontalImpulse.Length() > MaxHorizontalSpeed)
                horizontalImpulse = horizontalImpulse.Normalized() * MaxHorizontalSpeed;

            Body.Velocity = new Vector3(horizontalImpulse.X, currentVel.Y + _knockbackVelocity.Y, horizontalImpulse.Z);
            _appliedOnce = true;
        }

        // 2. KLUCZ: Hamowanie (Damping) aplikowane bezpośrednio do Body.Velocity!
        if (_appliedOnce)
        {
            Vector3 v = Body.Velocity;
            // Hamujemy tylko X i Z, grawitację zostawiamy ruchowi/stanowi
            float horizontalLength = new Vector2(v.X, v.Z).Length();

            if (horizontalLength > ActiveThreshold)
            {
                // Wykładnicze hamowanie prędkości ciała
                float speedDrop = Mathf.Exp(-Damping * dt);
                Body.Velocity = new Vector3(v.X * speedDrop, v.Y, v.Z * speedDrop);
            }
            else
            {
                // Zatrzymujemy całkowicie, gdy prędkość jest znikoma
                Body.Velocity = new Vector3(0, v.Y, 0);
                _knockbackVelocity = Vector3.Zero; // Reset wewnętrzny
            }
        }
    }
}
