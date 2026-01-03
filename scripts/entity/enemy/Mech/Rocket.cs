using Godot;

public partial class Rocket : CharacterBody3D
{
    [Export] public HitboxComponent Hitbox { get; set; }

    [Export] public float Speed { get; set; } = 30f;
    [Export] public float TurnSpeed { get; set; } = 3f;
    [Export] public float LifeTime { get; set; } = 6f;
    [Export] public float ExplosionDuration { get; set; } = 2f;
    [Export] public float ExplosionRadius { get; set; } = 2f;

    private HealthComponent _health;
    private HitboxComponent _hitbox;
    private Shape3D _hitboxShape;

    [Export] public MeshInstance3D _rocketMesh;
    [Export] public MeshInstance3D _flame1, _flame2, _flame3;

    private ExplosionVFX _explosionVfx;
    private Node3D _target;
    private Vector3 _moveDir = Vector3.Forward;
    private float _timeAlive;
    private bool _exploded;

    public override void _Ready()
    {
        _explosionVfx = GetNode<ExplosionVFX>("ExplosionVFX");
        _health = GetNodeOrNull<HealthComponent>("HealthComponent");
        if (_health != null)
            _health.EntityDied += OnDied;

        _hitbox = GetNode<HitboxComponent>("HitboxComponent");

        var shapeNode = _hitbox.GetNode<CollisionShape3D>("CollisionShape3D");
        _hitboxShape = shapeNode.Shape;

        if (_hitboxShape is SphereShape3D sphere)
            sphere.Radius = ExplosionRadius;

        _hitbox.Active = false;

        _target = GetTree().GetFirstNodeInGroup("player") as Node3D;

        if (_moveDir == Vector3.Zero)
            _moveDir = GlobalTransform.Basis.Y.Normalized();
    }

    private void OnDied()
    {
        Explode();
    }

    public void Initialize(Vector3 initialDir)
    {
        _moveDir = initialDir.Normalized();
        LookAt(GlobalPosition + _moveDir, Vector3.Up);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_exploded)
            return;

        float dt = (float)delta;
        _timeAlive += dt;

        if (_timeAlive >= LifeTime)
        {
            var mesh = GetNode<MeshInstance3D>("RocketMesh");
            var mat = mesh.GetSurfaceOverrideMaterial(0).Duplicate() as StandardMaterial3D;
            mesh.SetSurfaceOverrideMaterial(0, mat);

            mat.EmissionEnabled = true;
            mat.Emission = new Color(1, 1, 1, 0); // przezroczyste emission

            var tween = CreateTween();
            // Stopniowo zwiększamy intensywność emisji → ładniejsza linia do wybuchu
            tween.TweenProperty(mat, "emission_energy_multiplier", 1f, 1.0f); 
            tween.TweenCallback(Callable.From(() => Explode()));
        }

        if (_target != null && IsInstanceValid(_target))
        {
            Vector3 toTarget = _target.GlobalPosition - GlobalPosition;
            toTarget.Y += 1.0f; // celowanie trochę wyżej, np. w klatę

            if (toTarget != Vector3.Zero)
            {
                Vector3 desiredDir = toTarget.Normalized();
                float t = Mathf.Clamp(TurnSpeed * dt, 0f, 1f);
                _moveDir = _moveDir.Slerp(desiredDir, t).Normalized();

                LookAt(GlobalPosition + _moveDir, Vector3.Up, true);
            }
        }

        Velocity = _moveDir * Speed;

        var collision = MoveAndCollide(Velocity * dt);
        if (collision != null)
        {
            Explode();
        }
    }

    private void Explode(bool hurtEnemies = false)
    {
        if (_exploded)
            return;

        _explosionVfx.Explode();
        HideMeshes();

        _exploded = true;
        if(hurtEnemies == true)
        {
            _hitbox.CollisionLayer |= PhysicsLayers.PLAYER_HITBOX;
        }

        Velocity = Vector3.Zero;
        SetPhysicsProcess(false);

        _hitbox.Active = true;

        GetTree().CreateTimer(ExplosionDuration).Timeout += () =>
        {
            QueueFree();
        };
    }

    private void HideMeshes()
    {
        _rocketMesh.Visible = false;
        _flame1.Visible = false;
        _flame2.Visible = false;
        _flame3.Visible = false;
    }
}

