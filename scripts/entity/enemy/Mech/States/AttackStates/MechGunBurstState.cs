using Godot;

public class MechGunBurstState : IState
{
    private readonly Mech _owner;

    private int _shotsLeft;
    private float _timer;
    private bool _nextLeft = true;

    private const int ShotsPerBurst = 12;
    private const float ShotInterval = 0.25f;

    public MechGunBurstState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        GD.Print("Enter GUN State");
        _shotsLeft = ShotsPerBurst;
        _timer = 0f;
        _owner.ResetGunCooldown();
    }

    public void Exit()
    {
        _owner.ResetGunCooldown();
        // nic specjalnego – AI i tak przełączy się z powrotem na None
    }

    public void Update(double delta)
    {
        if (_shotsLeft <= 0)
        {
            _owner.ChangeAttackState(MechAttackStateId.None);
            return;
        }

        _timer -= (float)delta;
        if (_timer <= 0f)
        {
            FireShot();
            _timer = ShotInterval;
            _shotsLeft--;
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

    public void PhysicsUpdate(double delta) { }
}
