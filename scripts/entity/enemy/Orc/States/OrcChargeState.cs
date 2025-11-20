using Godot;

public class OrcChargeState : IState
{
    private readonly Orc _owner;
    private float _elapsed;
    private Vector3 _chargeDir; // aktualny kierunek szarży
    private float _prevAcceleration;
    private float _speedLerp; // 0..1 – jak bardzo zbliżyliśmy się do pełnej prędkości

    public OrcChargeState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _elapsed = 0f;
        _speedLerp = 0f;

        _owner.Pathfind.Active = false;
        _owner.Animation.Play("Orc_Charge");

        // zapamiętujemy stare przyspieszenie i ustawiamy "cięższe" przyspieszanie do szarży
        _prevAcceleration = _owner.VelocityComp.Acceleration;
        _owner.VelocityComp.Acceleration = _owner.ChargeAcceleration;

        // na starcie – prędkość maksymalna jak przy biegu
        _owner.VelocityComp.MaxSpeed = _owner.ChaseSpeed;

        // Ustal początkowy kierunek szarży
        Vector3 toPlayer;
        if (_owner.Player != null)
            toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        else
            toPlayer = _owner.GlobalTransform.Basis.Z; // fallback: do przodu

        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() < 0.001f)
            _chargeDir = _owner.GlobalTransform.Basis.Z;
        else
            _chargeDir = toPlayer.Normalized();

        // Obracamy orka zgodnie z kierunkiem szarży (tylko poziomo)
        Vector3 lookTarget = _owner.GlobalPosition + _chargeDir;
        _owner.LookAt(lookTarget, Vector3.Up, true);

        // Od razu zaczynamy ruch w tym kierunku
        _owner.VelocityComp.SetDesiredDirection(_chargeDir);
    }

    public void Exit()
    {
        // przywracamy normalne przyspieszenie
        _owner.VelocityComp.Acceleration = _prevAcceleration;

        // po szarży nie chcemy dalej mieć SetDesiredDirection w przód
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;
        _elapsed += dt;

        if (_owner.Player == null)
        {
            _owner.ChangeState(OrcStateId.Stop);
            return;
        }

        // łagodny ramp-up prędkości maksymalnej z ChaseSpeed -> ChargeSpeed
        // ChargeRampSpeed: jak szybko dobijamy do pełnej prędkości szarży
        const float chargeRampSpeed = 1.5f; // możesz wyciągnąć do [Export] w Orc
        _speedLerp = Mathf.Clamp(_speedLerp + chargeRampSpeed * dt, 0f, 1f);

        float currentMaxSpeed = Mathf.Lerp(_owner.ChaseSpeed, _owner.ChargeSpeed, _speedLerp);
        _owner.VelocityComp.MaxSpeed = currentMaxSpeed;

        // kierunek do gracza w poziomie
        Vector3 toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() > 0.001f)
        {
            Vector3 targetDir = toPlayer.Normalized();

            // ograniczony skręt – wolne doginanie kierunku do gracza
            _chargeDir = _chargeDir.Slerp(targetDir, _owner.ChargeTurnSpeed * dt).Normalized();

            // obrót tylko horyzontalny
            Vector3 lookTarget = _owner.GlobalPosition + _chargeDir;
            _owner.LookAt(lookTarget, Vector3.Up, true);

            // ruch w przód – VelocityComp rozpędza do currentMaxSpeed
            _owner.VelocityComp.SetDesiredDirection(_chargeDir);
        }

        if (ShouldStopCharge())
        {
            _owner.ChangeState(OrcStateId.Stop);
        }
    }

    public void Update(double delta) { }

    private bool ShouldStopCharge()
    {
        // 1. Zbyt długo szarżuje
        if (_elapsed >= _owner.MaxChargeTime)
            return true; // Tu fajnie jakby dodać przejście do Chase;

        // 2. Zaraz wbije w przeszkodę – patrzymy w KIERUNKU SZARŻY
        var space = _owner.GetWorld3D().DirectSpaceState;

        Vector3 from = _owner.GlobalPosition + Vector3.Up * 0.5f;
        Vector3 forward = _chargeDir; // faktyczny kierunek biegu
        Vector3 to = from + forward * _owner.ObstacleCheckDistance;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollisionMask = _owner.ObstacleMask;
        query.Exclude = new Godot.Collections.Array<Rid> { _owner.GetRid() };

        var result = space.IntersectRay(query);
        return result.Count > 0;
    }
}
