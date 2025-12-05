using Godot;

public class MechRocketVolleyState : IState
{
    private readonly Mech _owner;
    private int _rocketsLeft;
    private float _timer;
    private bool _nextLeft = true;

    private const int RocketsPerVolley = 6;
    private const float RocketInterval = 1.2f;

    public MechRocketVolleyState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _rocketsLeft = RocketsPerVolley;
        _timer = 0f;
        _owner.ResetRocketCooldown();
    }

    public void Exit()
    {
        _owner.ResetRocketCooldown();
    }

    public void Update(double delta)
    {
        if (_rocketsLeft <= 0)
        {
            _owner.ChangeAttackState(MechAttackStateId.None);
            return;
        }

        _timer -= (float)delta;
        if (_timer <= 0f)
        {
            FireRocket();
            _timer = RocketInterval;
            _rocketsLeft--;
        }
    }

    private void FireRocket()
    {
        if (_nextLeft)
            _owner.PlayUpperBody("Mech_ShootLSlow");
        else
            _owner.PlayUpperBody("Mech_ShootRSlow");

        _owner.SpawnRocket(_nextLeft);
        _nextLeft = !_nextLeft;
    }

    public void PhysicsUpdate(double delta) { }
}
