// using Godot;
// using System;
// using System.Collections.Generic;

// public partial class Rocket : CharacterBody3D
// {
//     [Export] public float Speed { get; set; } = 30f;
//     [Export] public float TurnSpeed { get; set; } = 3f;
//     [Export] public float LifeTime { get; set; } = 6f;
//     [Export] public float ExplosionDuration { get; set; } = 0.15f;
//     [Export] public float ExplosionRadius { get; set; } = 5f;

//     private HurtboxComponent _hurtbox;
//     private HealthComponent _health;
//     private HitboxComponent _hitbox;
//     private Shape3D _hitboxShape;

//     private Node3D _target;
//     private Vector3 _moveDir = Vector3.Forward;
//     private float _timeAlive;
//     private bool _exploded;

//     public override void _Ready()
//     {
//         _health = GetNode<HealthComponent>("HealthComponent");
//         _target = GetTree().GetFirstNodeInGroup("player") as Node3D;
//         _hitbox = GetNode<HitboxComponent>("HitboxComponent");
//         _hitboxShape = GetNode<CollisionShape3D>("CollisionShape3D").Shape;


//         ((SphereShape3D)_hitboxShape).Radius = ExplosionRadius;
//         _hitbox.Active = false;


//         if (_moveDir == Vector3.Zero)
//             _moveDir = GlobalTransform.Basis.Y.Normalized();

//         _health.EntityDied += OnDied;
//     }

//     private void OnDied()
//     {
//         Explode(true);
//     }


//     public void Initialize(Vector3 initialDir)
//     {
//         _moveDir = initialDir.Normalized();
//         LookAt(GlobalPosition + _moveDir, Vector3.Up);
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         float dt = (float)delta;
//         _timeAlive += dt;

//         if (_timeAlive >= LifeTime)
//         {
//             Explode();
//             return;
//         }

//         if (_target != null && IsInstanceValid(_target))
//         {
//             Vector3 toTarget = _target.GlobalPosition - GlobalPosition;
//             toTarget.Y += 1.0f;

//             if (toTarget != Vector3.Zero)
//             {
//                 Vector3 desiredDir = toTarget.Normalized();
//                 float t = Mathf.Clamp(TurnSpeed * dt, 0f, 1f);
//                 _moveDir = _moveDir.Slerp(desiredDir, t).Normalized();
//                 LookAt(GlobalPosition + _moveDir, Vector3.Up, true);
//             }
//         }

//         GlobalPosition += _moveDir * Speed * dt;
//         MoveAndCollide();
//     }

//     private void Explode(bool hurtEnemies = false)
//     {
//         // Wyłącz bezpośrednie kolizje, żeby nic więcej nie łapać
//         if(hurtEnemies == true)
//         {
//             CollisionLayer |= PhysicsLayers.PLAYER_HITBOX;
//         }

//         _hitArea.SetDeferred("monitoring", false);

//         // Ustaw AoE w miejscu eksplozji
//         GlobalPosition = GlobalPosition;

//         SetDeferred("monitoring", true);

//         GetTree().CreateTimer(0.05f).Timeout += () =>
//         {
//             QueueFree();
//         };
//     }

//     // ===== IDamageSource =====

//     public float GetDamage() => _damage;

//     public Element GetDamageType() => _damageType;

//     public bool CanHitAgain(Node3D target)
//     {
//         if (RehitCooldownSeconds <= 0f)
//             return true;

//         if (!_lastHitTimeByTarget.TryGetValue(target, out var lastTime))
//             return true;

//         return _timeAlive - (float)lastTime >= RehitCooldownSeconds;
//     }

//     public void RegisterHit(Node3D target)
//     {
//         if (RehitCooldownSeconds <= 0f)
//             return;

//         _lastHitTimeByTarget[target] = _timeAlive;
//     }
// }

using Godot;
using System;

public partial class Rocket : CharacterBody3D
{
    // ===== RUCH =====
    [Export] public float Speed { get; set; } = 30f;
    [Export] public float TurnSpeed { get; set; } = 3f;
    [Export] public float LifeTime { get; set; } = 6f;
    [Export] public float ExplosionDuration { get; set; } = 0.15f;
    [Export] public float ExplosionRadius { get; set; } = 5f;

    // ===== REFERENCJE =====
    private HealthComponent _health;
    private HitboxComponent _hitbox;
    private Shape3D _hitboxShape;

    private Node3D _target;
    private Vector3 _moveDir = Vector3.Forward;
    private float _timeAlive;
    private bool _exploded;

    public override void _Ready()
    {
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

        _exploded = true;
        if(hurtEnemies == true)
        {
            _hitbox.CollisionLayer |= PhysicsLayers.PLAYER_HITBOX;
        }

        Velocity = Vector3.Zero;
        SetPhysicsProcess(false);

        _hitbox.Active = true;

        // Po ExplosionDuration sprzątamy całą scenę rakiety (wraz z hitboxem)
        GD.Print("EXPLODE");
        GetTree().CreateTimer(ExplosionDuration).Timeout += () =>
        {
            QueueFree();
        };
    }
}

