using Godot;

public partial class BeamEmitter : Node3D
{
    [Export] public NodePath HitboxPath = "Hitbox";
    [Export] public NodePath RayCastPath = "RayCast";
    [Export] public NodePath CollisionShapePath = "Hitbox/CollisionShape3D";

    [Export] public float MaxLength = 20f;

    private HitboxComponent hitbox;
    private RayCast3D rayCast;
    private CollisionShape3D collisionShape;

    public override void _Ready()
    {
        hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
        rayCast = GetNodeOrNull<RayCast3D>(RayCastPath);
        collisionShape = GetNodeOrNull<CollisionShape3D>(CollisionShapePath);

        if (hitbox == null) GD.PrintErr(Name + ": BeamEmitter missing HitboxComponent");
        if (rayCast == null) GD.PrintErr(Name + ": BeamEmitter missing RayCast3D");
        if (collisionShape == null) GD.PrintErr(Name + ": BeamEmitter missing CollisionShape3D at " + CollisionShapePath);
    }

    public void Configure(Element element, float damagePerTick, float tickRateSeconds, float maxLength)
    {
        MaxLength = maxLength;

        if (hitbox != null)
        {
            hitbox.Damage = damagePerTick;
            hitbox.DamageType = element;
            hitbox.OneShot = false;
            hitbox.RehitCooldownSeconds = Mathf.Max(0.01f, tickRateSeconds);
        }
    }

    public void UpdateBeam(Node3D muzzle, Vector3 direction)
    {
        if (muzzle == null || rayCast == null)
            return;

        GlobalTransform = muzzle.GlobalTransform;
        LookAt(GlobalPosition + direction.Normalized(), Vector3.Up);

        rayCast.TargetPosition = new Vector3(0, 0, -MaxLength);
        rayCast.ForceRaycastUpdate();

        float length = MaxLength;
        if (rayCast.IsColliding())
        {
            Vector3 hitPoint = rayCast.GetCollisionPoint();
            length = GlobalPosition.DistanceTo(hitPoint);
        }

        UpdateHitboxLength(length);
    }

    private void UpdateHitboxLength(float length)
    {
        if (collisionShape == null)
            return;

        // zakładamy BoxShape3D jako “rura”
        BoxShape3D box = collisionShape.Shape as BoxShape3D;
        if (box == null)
            return;

        Vector3 size = box.Size;
        size.Z = length;
        box.Size = size;

        // przesuwamy hitbox tak, żeby zaczynał przy muzzle i leciał do przodu
        // Godot Z- to przód kamery, więc w lokalnym: środek boxa jest w połowie długości
        collisionShape.Position = new Vector3(0, 0, -length * 0.5f);
    }
}
