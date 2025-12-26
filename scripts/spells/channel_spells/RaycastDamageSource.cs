// using Godot;
// using System.Collections.Generic;

// public partial class RaycastDamageSource : Node3D, IDamageSource
// {
//     [Export] public float Damage { get; set; } = 20f;
//     [Export] public Element DamageType { get; set; } = Element.None;
//     [Export] public float RehitCooldownSeconds { get; set; } = 0.1f;
//     [Export] public bool OneShot { get; set; } = false;

//     private readonly Dictionary<Node3D, double> _alreadyHit = new();
//     private double _timeAlive;

//     public override void _PhysicsProcess(double delta) => _timeAlive += delta;

//     public float GetDamage() => Damage;
//     public Element GetDamageType() => DamageType;

//     public bool CanHitAgain(Node3D target)
//     {
//         if (OneShot)
//             return !_alreadyHit.ContainsKey(target);

//         if (RehitCooldownSeconds <= 0f)
//             return true;

//         if (!_alreadyHit.TryGetValue(target, out var lastTime))
//             return true;

//         return _timeAlive - lastTime >= RehitCooldownSeconds;
//     }

//     public void RegisterHit(Node3D target) => _alreadyHit[target] = _timeAlive;
// }
