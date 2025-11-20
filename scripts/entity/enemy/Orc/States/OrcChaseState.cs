using Godot;

public class OrcChaseState : IState
{
    private readonly Orc _owner;
    private float _chaseTime;

    public OrcChaseState(Orc owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _chaseTime = 0f;

        _owner.Pathfind.Active = true;
        _owner.VelocityComp.MaxSpeed = _owner.ChaseSpeed;
        _owner.Animation.Play("Orc_SlowRun");
    }

    public void Exit()
    {
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.SetDesiredDirection(Vector3.Zero);
    }

    public void PhysicsUpdate(double delta)
    {
        var dt = (float)delta;
        _chaseTime += dt;

        if (_owner.Player == null)
            return;

        // Obrót zgodny z ruchem (ładny, płynny)
        _owner.VelocityComp.RotateTowardsMovement((float)delta);

        if (CanStartCharge())
        {
            _owner.ChangeState(OrcStateId.Charge);
        }
    }

    public void Update(double delta) { }

    private bool CanStartCharge()
    {
        if(_chaseTime < _owner.ChargeCooldown)
            return false;

        Vector3 toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        Vector3 horizontalToPlayer = toPlayer;
        horizontalToPlayer.Y = 0;

        float distance = horizontalToPlayer.Length();
        if (distance < _owner.ChargeMinDistance || distance > _owner.ChargeMaxDistance)
            return false;

        if (horizontalToPlayer.LengthSquared() < 0.001f)
            return false;

        // FOV – czy gracz jest mniej więcej przed Orciem
        Vector3 desiredDir = horizontalToPlayer.Normalized();
        Vector3 forward = _owner.GlobalTransform.Basis.Z; // zależnie od modelu, czasem +Z

        float dot = desiredDir.Dot(forward);
        if (dot < _owner.ChargeFovDotThreshold)
            return false;

        // Line of Sight – raycast do gracza
        var space = _owner.GetWorld3D().DirectSpaceState;
        Vector3 from = _owner.GlobalPosition + Vector3.Up * 1f;
        Vector3 to = _owner.Player.GlobalPosition + Vector3.Up * 1f;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.Exclude = new Godot.Collections.Array<Rid> { _owner.GetRid() };
        
        query.CollisionMask = _owner.LineOfSightMask; // świat + gracz

        var result = space.IntersectRay(query);
        if (result.Count == 0)
            return false;

        var collider = result["collider"].As<Node3D>();
        if (collider == null)
            return false;

        // jeśli pierwszym trafieniem jest gracz -> mamy czystą linię
        return collider.IsInGroup("player");
    }
}
