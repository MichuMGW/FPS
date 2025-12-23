// using Godot;

// public partial class ScalingProjectile : Projectile
// {
//     [Export] public float StartScale = 0.5f;
//     [Export] public float EndScale = 2.0f;
//     [Export] public float ScaleDuration = 1.0f; // sekundy, 0 = natychmiast

//     [Export] public NodePath VisualPath = "Visual"; // Node3D z meshem (albo sam mesh)
//     [Export] public NodePath CollisionShapePath = "CollisionShape3D";

//     private Node3D _visual;
//     private CollisionShape3D _cs;

//     private SphereShape3D _sphere;
//     private CapsuleShape3D _capsule;
//     private float _t;

//     private float _baseSphereRadius;
//     private float _baseCapsuleRadius;
//     private float _baseCapsuleHeight;

//     public override void _Ready()
//     {
//         base._Ready();

//         _visual = GetNodeOrNull<Node3D>(VisualPath);
//         _cs = GetNodeOrNull<CollisionShape3D>(CollisionShapePath);

//         _cs.Shape = (Shape3D)_cs.Shape.Duplicate(true);

//         if (_cs?.Shape is SphereShape3D s)
//         {
//             _sphere = s;
//             _baseSphereRadius = s.Radius;
//         }
//         else if (_cs?.Shape is CapsuleShape3D c)
//         {
//             _capsule = c;
//             _capsule.Radius = c.Radius;
//             _baseCapsuleHeight = c.Height;
//         }

//         ApplyScale(StartScale);
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         base._PhysicsProcess(delta);
//     }

//     protected override void OnBeforeMove(float dt)
//     {
//         _t += dt;
//         float a = (ScaleDuration <= 0f) ? 1f : Mathf.Clamp(_t / ScaleDuration, 0f, 1f);
//         float s = Mathf.Lerp(StartScale, EndScale, a);
//         ApplyScale(s);
//     }

//     private void ApplyScale(float s)
//     {
//         if (_visual != null)
//             _visual.Scale = Vector3.One * s;

//         // Fizyka: skaluj parametry shape (nie global scale)
//         if (_sphere != null)
//         {
//             _sphere.Radius = _baseSphereRadius * s;
//         }
//         else if (_capsule != null)
//         {
//             _capsule.Height = _baseCapsuleHeight * s;
//         }
//     }
// }
