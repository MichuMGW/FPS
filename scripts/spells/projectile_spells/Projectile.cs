using Godot;

public partial class Projectile : CharacterBody3D
{
    public Element Element { get; set; }
    public float Damage { get; set; }
    public float Range { get; set; }
    public float LifeTime { get; set; } = 5f;

    private Vector3 _startPosition;
    private float _timeAlive;

    public void Initialize(Vector3 velocity)
    {
        Velocity = velocity;
        _startPosition = GlobalTransform.Origin;
        _timeAlive = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        _timeAlive += dt;
        if (_timeAlive >= LifeTime)
        {
            QueueFree();
            return;
        }

        if (_startPosition.DistanceTo(GlobalTransform.Origin) > Range)
        {
            QueueFree();
            return;
        }

        var collision = MoveAndCollide(Velocity * dt);
        if (collision != null)
        {
            // TODO: tu docelowo: ApplyDamage(collision.GetCollider()) / HitboxComponent
            QueueFree();
        }
    }
}
