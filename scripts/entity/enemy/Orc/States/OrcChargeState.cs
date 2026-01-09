using Godot;

public class OrcChargeState : IState, IPhysicsUpdateState
{
    private readonly Orc _owner;
    private float _timeElapsed;
    private Vector3 _chargeDir;
    private float _prevAcceleration;
    private float _speedLerp;

    public OrcChargeState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _timeElapsed = 0f;
        _speedLerp = 0f;

        _owner.ChargeHitbox.Active = true;

        _owner.Pathfind.Active = false;
        _owner.Pathfind.SetPhysicsProcess(false);
        _owner.Animation.Play("Orc_Charge", 0.5f);

        // Zapamiętanie starego przyspieszenia i ustawienie wartości przyspieszenia dla szarży
        _prevAcceleration = _owner.VelocityComp.Acceleration;
        _owner.VelocityComp.Acceleration = _owner.ChargeAcceleration;

        _owner.VelocityComp.MaxSpeed = _owner.ChaseSpeed;

        // Ustalenie początkowego kierunku szarży
        Vector3 toPlayer;

        if (_owner.Player != null)
            toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        else
            toPlayer = _owner.GlobalTransform.Basis.Z;

        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() < 0.001f)
            _chargeDir = _owner.GlobalTransform.Basis.Z;
        else
            _chargeDir = toPlayer.Normalized();

        Vector3 lookTarget = _owner.GlobalPosition + _chargeDir;
        _owner.LookAt(lookTarget, Vector3.Up, true);

        _owner.VelocityComp.SetDesiredDirection(_chargeDir);
    }

    public void Exit()
    {
        _owner.VelocityComp.Acceleration = _prevAcceleration;

        _owner.ChargeHitbox.Active = false;

        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
        _owner.Pathfind.SetPhysicsProcess(true);
    }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;
        _timeElapsed += dt;

        if (_owner.Player == null)
        {
            _owner.ChangeState(OrcStateId.Stop);
            return;
        }


        // Łagodne przejście z prędkości biegu do prędkości szarży
        // ChargeRampSpeed decyduje o tym jak szybko przeciwnik osiąga maksymalną prędkość szarży
        _speedLerp = Mathf.Clamp(_speedLerp + _owner.ChargeRampSpeed * dt, 0f, 1f);

        float currentMaxSpeed = Mathf.Lerp(_owner.ChaseSpeed, _owner.ChargeSpeed, _speedLerp);
        _owner.VelocityComp.MaxSpeed = currentMaxSpeed;

        Vector3 toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() > 0.001f)
        {
            Vector3 targetDir = toPlayer.Normalized();

            // Określenie kierunku szarży z ograniczoną skrętnością w kierunku celu
            _chargeDir = _chargeDir.Slerp(targetDir, _owner.ChargeTurnSpeed * dt).Normalized();

            Vector3 lookTarget = _owner.GlobalPosition + _chargeDir;
            _owner.LookAt(lookTarget, Vector3.Up, true);

            _owner.VelocityComp.SetDesiredDirection(_chargeDir);
        }

        if (ShouldStopCharge())
        {
            _owner.ChangeState(OrcStateId.Stop);
        }
    }

    private bool ShouldStopCharge()
    {
        if (_timeElapsed >= _owner.MaxChargeTime)
            return true;

        // Dostęp do silnika fizycznego w świecie 3D
        var space = _owner.GetWorld3D().DirectSpaceState;

        Vector3 from = _owner.GlobalPosition + Vector3.Up * 0.5f;
        Vector3 forward = _chargeDir; // faktyczny kierunek biegu
        Vector3 to = from + forward * _owner.ObstacleCheckDistance;

        // Stworzenie zapytania sprawdzającego kolizje z obiektami o masce ObstacleMask
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollisionMask = _owner.ObstacleMask;
        query.Exclude = new Godot.Collections.Array<Rid> { _owner.GetRid() };
        var result = space.IntersectRay(query);

        return result.Count > 0;
    }
}
