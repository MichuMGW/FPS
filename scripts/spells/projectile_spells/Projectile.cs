using Godot;
using System.Collections.Generic;

public partial class Projectile : CharacterBody3D
{
    [Export] public NodePath HitboxPath = "Hitbox";
    [Export] public NodePath CollisionShapePath = "CollisionShape3D";
    [Export] public NodePath VisualPath = "Visual"; // jeśli nie masz, zostaw puste i będzie skalował root

    [Export] public float LifeTime = 5f;

    private HitboxComponent _hitbox;
    private CollisionShape3D _collisionShape;
    private Node3D _visual;

    // core stats
    private Element _element;
    private float _damage;
    private float _range;
    private Vector3 _startPosition;
    private float _timeAlive;

    // pierce
    private int _remainingPierce;
    private readonly HashSet<ulong> _piercedTargets = new();

    // rehit (tick damage kiedy pocisk “siedzi” w celu)
    private bool _rehitEnabled;
    private float _rehitInterval;
    private float _rehitAcc;

    // scaling
    private bool _scaleEnabled;
    private float _scaleStart;
    private float _scaleEnd;
    private float _scaleDuration;
    private float _scaleT;

    // bazowe parametry shape (z inspektora, ale na sklonowanym zasobie)
    private SphereShape3D _sphere;
    private CapsuleShape3D _capsule;
    private BoxShape3D _box;

    private float _baseSphereRadius;
    private float _baseCapsuleRadius;
    private float _baseCapsuleHeight;
    private Vector3 _baseBoxSize;

    // explosion
    private bool _explodeOnEnemy;
    private bool _explodeOnWorld;
    private PackedScene _explosionScene;
    private float _explosionDamageMult;

    private bool _dead;
    private bool _exploded;

    public override void _Ready()
    {
        _hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
        _collisionShape = GetNodeOrNull<CollisionShape3D>(CollisionShapePath);

        _visual = GetNodeOrNull<Node3D>(VisualPath);
        if (_visual == null)
            _visual = this; // fallback: skaluje root

        if (_hitbox == null)
        {
            GD.PushError($"{Name}: Projectile requires HitboxComponent at '{HitboxPath}'.");
            _dead = true;
            return;
        }

        if (_collisionShape == null || _collisionShape.Shape == null)
        {
            GD.PushWarning($"{Name}: Missing CollisionShape3D/Shape at '{CollisionShapePath}'. Scaling collider will be skipped.");
        }
        else
        {
            // KLUCZ: duplikujemy zasób shape per instancja, żeby nie “pamiętał” między castami
            _collisionShape.Shape = (Shape3D)_collisionShape.Shape.Duplicate(true);

            if (_collisionShape.Shape is SphereShape3D s)
            {
                _sphere = s;
                _baseSphereRadius = s.Radius;
            }
            else if (_collisionShape.Shape is CapsuleShape3D c)
            {
                _capsule = c;
                _baseCapsuleRadius = c.Radius;
                _baseCapsuleHeight = c.Height;
            }
            else if (_collisionShape.Shape is BoxShape3D b)
            {
                _box = b;
                _baseBoxSize = b.Size;
            }
        }

        // bez lambd, jak prosiłeś
        _hitbox.AreaEntered += OnHitboxAreaEntered;

        // hitbox ma sens tylko gdy monitoruje
        _hitbox.Active = true;
    }

    // Jedno Configure dla wszystkiego. Spell/modifier może to ustawić jak chce.
    public void Configure(
        Element element,
        float damage,
        float range,
        int pierce,

        // rehit
        bool enableRehit = false,
        float rehitIntervalSeconds = 0.25f,

        // scaling
        bool scaleOverTime = false,
        float startScale = 1f,
        float endScale = 1f,
        float scaleDurationSeconds = 0f,

        // explosion
        PackedScene explosionScene = null,
        float explosionDamageMultiplier = 1f,
        bool explodeOnEnemy = false,
        bool explodeOnWorld = false,

		bool dieOnWorldHit = true,
		float explosionLifetimeSeconds = 0f
    )
    {
        _element = element;
        _damage = damage;
        _range = range;

        _remainingPierce = pierce;
        _piercedTargets.Clear();

        // hitbox dmg config
        if (_hitbox != null)
        {
            _hitbox.Damage = _damage;
            _hitbox.DamageType = _element;

            // OneShot musi być false, bo inaczej gating w HitboxComponent uwali kolejne cele/rehit
            _hitbox.OneShot = !enableRehit;

            // rehit gating robimy przez HitboxComponent
            _hitbox.RehitCooldownSeconds = enableRehit ? Mathf.Max(0.01f, rehitIntervalSeconds) : 0f;
        }

        // rehit
        _rehitEnabled = enableRehit;
        _rehitInterval = Mathf.Max(0.01f, rehitIntervalSeconds);
        _rehitAcc = 0f;

        // scaling
        _scaleEnabled = scaleOverTime;
        _scaleStart = startScale;
        _scaleEnd = endScale;
        _scaleDuration = Mathf.Max(0f, scaleDurationSeconds);
        _scaleT = 0f;

        if (_scaleEnabled)
            ApplyScale(_scaleStart);

        // explosion
        _explosionScene = explosionScene;
        _explosionDamageMult = explosionDamageMultiplier;
        _explodeOnEnemy = explodeOnEnemy;
        _explodeOnWorld = explodeOnWorld;

        _exploded = false;
    }

    public void Launch(Vector3 velocity)
    {
        Velocity = velocity;
        _startPosition = GlobalPosition;
        _timeAlive = 0f;
        _dead = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_dead)
            return;

        float dt = (float)delta;

        _timeAlive += dt;
        if (_timeAlive >= LifeTime)
        {
            QueueFree();
            _dead = true;
            return;
        }

        if (_startPosition.DistanceTo(GlobalPosition) > _range)
        {
            QueueFree();
            _dead = true;
            return;
        }

        // scaling tick
        if (_scaleEnabled)
            TickScale(dt);

        // move + world collision
        var collision = MoveAndCollide(Velocity * dt);
        if (collision != null)
        {
            if (_explodeOnWorld && !_exploded)
            {
                // world hit: wybuch w punkcie kontaktu
                ExplodeAt(collision.GetPosition());
                return;
            }

            // jeśli nie wybucha na world, to standardowo kończ (albo możesz odbijać, twoja bajka)
            QueueFree();
            _dead = true;
            return;
        }

        // rehit tick (polling overlapy)
        if (_rehitEnabled && !_exploded)
		{
            TickRehit(dt);
		}
    }

    private void TickScale(float dt)
    {
        _scaleT += dt;

        float a = (_scaleDuration <= 0f) ? 1f : Mathf.Clamp(_scaleT / _scaleDuration, 0f, 1f);
        float s = Mathf.Lerp(_scaleStart, _scaleEnd, a);

        ApplyScale(s);
    }

    private void ApplyScale(float s)
    {
        if (_visual != null)
            _visual.Scale = Vector3.One * s;

        // collider: skalujemy parametry shape, nie global scale (stabilniej)
        if (_sphere != null)
        {
            _sphere.Radius = _baseSphereRadius * s;
        }
        else if (_capsule != null)
        {
            _capsule.Radius = _baseCapsuleRadius * s;
            _capsule.Height = _baseCapsuleHeight * s;
        }
        else if (_box != null)
        {
            _box.Size = _baseBoxSize * s;
        }
    }

    private void TickRehit(float dt)
    {
        _rehitAcc += dt;
        if (_rehitAcc < _rehitInterval)
            return;

        _rehitAcc -= _rehitInterval;

        if (_hitbox == null || !_hitbox.Monitoring)
            return;

        var overlapped = _hitbox.GetOverlappingAreas();
        if (overlapped == null || overlapped.Count == 0)
            return;

        for (int i = 0; i < overlapped.Count; i++)
        {
            if (overlapped[i] is not HurtboxArea hurtbox)
                continue;

            var ownerComp = hurtbox.OwnerHurtboxComponent;
            if (ownerComp == null)
                continue;

            var target = ownerComp.GetOwner<Node3D>();
            if (target == null)
                continue;

            // gating per target przez HitboxComponent
            if (!_hitbox.CanHitAgain(target))
                continue;

            ownerComp.ReceiveHit(hurtbox, _hitbox, hurtbox.GlobalPosition);
        }
    }

    private void OnHitboxAreaEntered(Area3D area)
    {
        if (_dead || _exploded)
            return;

        if (area is not HurtboxArea hurtbox)
            return;

        var ownerComp = hurtbox.OwnerHurtboxComponent;
        if (ownerComp == null)
            return;

        var target = ownerComp.GetOwner<Node3D>();
        if (target == null)
            return;

        ulong id = target.GetInstanceId();
        bool firstTimeThisTarget = _piercedTargets.Add(id);

        if (_explodeOnEnemy && firstTimeThisTarget)
        {
            ExplodeAt(hurtbox.GlobalPosition);
            return;
        }

        if (firstTimeThisTarget)
        {
            _remainingPierce -= 1;
            if (_remainingPierce < 0)
            {
                QueueFree();
                _dead = true;
                return;
            }
        }
    }


    private void ExplodeAt(Vector3 pos)
    {
        if (_exploded)
            return;

        _exploded = true;

        if (_explosionScene != null)
        {
            Node n = _explosionScene.Instantiate();
            if (n is Node3D n3)
            {
                GetTree().CurrentScene.AddChild(n3);
                n3.GlobalPosition = pos;

                // Jeśli scena explosion ma HitboxComponent i chcesz przeskalować dmg:
                var hb = n3.GetNodeOrNull<HitboxComponent>("HitboxComponent");
                if (hb != null)
                {
                    hb.Damage *= _explosionDamageMult;
                    hb.DamageType = _element;
                }
            }
            else
            {
                n.QueueFree();
                GD.PushWarning($"{Name}: ExplosionScene root should be Node3D.");
            }
        }

        QueueFree();
        _dead = true;
    }
}
