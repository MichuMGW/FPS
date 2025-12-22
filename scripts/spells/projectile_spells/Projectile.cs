using Godot;

public partial class Projectile : CharacterBody3D
{
	[Export] public NodePath HitboxPath = "Hitbox";
	[Export] public float LifeTime = 5f;

	private HitboxComponent hitbox;
	private Element element;
	private float damage;
	private float range;

	private Vector3 startPosition;
	private float timeAlive;
	private bool hasHitSomething;

	public override void _Ready()
	{
		hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
		if (hitbox == null)
		{
			GD.PrintErr(Name + ": Projectile requires HitboxComponent at path: " + HitboxPath);
			return;
		}

		GD.Print($"Projectile scale: {GlobalTransform.Basis.Scale}");
    var cs = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
    if (cs != null)
        GD.Print($"CollisionShape scale: {cs.GlobalTransform.Basis.Scale}");

    var parent = GetParentOrNull<Node3D>();
    if (parent != null)
        GD.Print($"Parent scale: {parent.GlobalTransform.Basis.Scale}");

		// bez lambd, jak prosiłeś
		hitbox.BodyEntered += OnHitboxBodyEntered;
		hitbox.AreaEntered += OnHitboxAreaEntered;
	}

	public void Configure(Element projectileElement, float projectileDamage, float projectileRange)
	{
		element = projectileElement;
		damage = projectileDamage;
		range = projectileRange;

		if (hitbox != null)
		{
			hitbox.Damage = damage;
			hitbox.DamageType = element;

			// typowe: pocisk trafia raz i znika
			hitbox.OneShot = true;
			hitbox.RehitCooldownSeconds = 0f;
		}
	}

	public void Launch(Vector3 velocity)
	{
		Velocity = velocity;
		startPosition = GlobalPosition;
		timeAlive = 0f;
		hasHitSomething = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		timeAlive += dt;
		if (timeAlive >= LifeTime)
		{
			QueueFree();
			return;
		}

		if (startPosition.DistanceTo(GlobalPosition) > range)
		{
			QueueFree();
			return;
		}

		var collision = MoveAndCollide(Velocity * dt);
        if (collision != null)
        {
            QueueFree();
        }
	}

	private void OnHitboxBodyEntered(Node body)
	{
		if (hasHitSomething)
			return;

		// jeśli chcesz ignorować własnego castera, dodaj tu filtr (np. grupa "player")
		hasHitSomething = true;
		QueueFree();
	}

	private void OnHitboxAreaEntered(Area3D area)
	{
		if (hasHitSomething)
			return;

		hasHitSomething = true;
		QueueFree();
	}
}
