using Godot;

public class MechSpinSpecialState : IState
{
    private readonly Mech _owner;

     private enum Phase
    {
        Crouch,
        Spin
    }
    private Phase _phase;
    private float _timer;
    private float _shotTimer;
    private bool _nextLeft = true;

    private const float Duration = 3.0f;
    private const float ShotInterval = 0.1f;
    private const float SpinSpeed = 6f; // rad/s lub coś w okolicy

    private const float CrouchDuration = 3.5f;
    private const float SpinDuration = 4f;
    private bool _spinClockwise; 

    public MechSpinSpecialState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        GD.Print("Enter SPIN State");
        _phase = Phase.Crouch;
        _timer = CrouchDuration;
        _shotTimer = 0f;

        _owner.LookAtActive = false;
        _owner.Pathfind.Active = false;
        _owner.VelocityComp.StopInstantly();

        _owner.PlayLocomotion("Mech_CrouchToShoot");
        // Możesz z AnimationPlayera przejść płynnie do Mech_Spin
        // albo tu po krótkim czasie zmienić anim:
        // _owner.PlayUpperBody("Mech_Spin");
    }

    public void Exit()
    {
        _owner.PlayUpperBody("Mech_Idle");
        _owner.PlayLocomotion("Mech_Idle");
        _owner.AddAfterSpecialCooldown();
        _owner.ResetSpinCooldown();
        // _owner.Pathfind.Active = true;
        // po zakończeniu specjalnego – AI znowu przejmuje ruch/ataki
    }

    public void Update(double delta)
    {
        float dt = (float)delta;
        
        _timer -= dt;

        switch (_phase)
        {
            case Phase.Crouch:
                if(_timer <= 0f)
                {
                    _phase = Phase.Spin;
                    _timer = SpinDuration;

                    _spinClockwise = ComputeSpinDirection();
                    
                    _owner.PlayLocomotion(_spinClockwise == true ? "Mech_SpinRight" : "Mech_SpinLeft");
                }
                break;
            case Phase.Spin:
                _shotTimer -= dt;
                if (_timer <= 0f)
                {
                    _owner.ChangeSuperState(MechSuperStateId.Normal);
                    return;
                }

                if (_shotTimer <= 0f)
                {
                    FireShot();
                    _shotTimer = ShotInterval;
                }
                break;
        }
        
    }

    private void FireShot()
    {
        if (_nextLeft)
            _owner.PlayUpperBody("Mech_ShootLFast");
        else
            _owner.PlayUpperBody("Mech_ShootRFast");

        _owner.SpawnGunProjectile(_nextLeft);
        _nextLeft = !_nextLeft;
    }

     private bool ComputeSpinDirection()
    {
        var forward = -_owner.GlobalTransform.Basis.Z;
        forward.Y = 0;

        if (forward.IsZeroApprox())
            forward = Vector3.Forward;

        forward = forward.Normalized();

        var toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        toPlayer.Y = 0;

        if (toPlayer.IsZeroApprox())
            return true; // domyślnie w prawo

        toPlayer = toPlayer.Normalized();

        float crossY = forward.Cross(toPlayer).Y;

        return crossY >= 0;
    }

    public void PhysicsUpdate(double delta)
    {
        // _owner.VelocityComp.StopInstantly();
    }
}
