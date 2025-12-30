using Godot;

public partial class ExplosionAoE : Node3D
{
    [Export] public NodePath HitboxPath = "HitboxComponent";
    [Export] public NodePath ShapePath = "HitboxComponent/CollisionShape3D";

    // Jak długo ma zostać na mapie (sekundy)
    [Export(PropertyHint.Range, "0.01,5,0.01")]
    public float LifeTime = 1;

    // Opcjonalnie: ścieżka do wizualnego mesha do skalowania
    [Export] public PackedScene ExplosionVfxScene;
    [Export(PropertyHint.Range, "0.1,10,0.1")]
    public float VfxLifeTime = 1f;

    private HitboxComponent hitbox;
    private CollisionShape3D shape;
    private Node3D visual;

    public override void _Ready()
    {
        hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
        shape = GetNodeOrNull<CollisionShape3D>(ShapePath);

        // Auto-destrukcja (bo inaczej zostaje wiecznie)
        if (LifeTime > 0f)
        {
            var timer = GetTree().CreateTimer(LifeTime);
            timer.Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(this))
                    QueueFree();
            };
        }
    }

    public void Configure(
        Element element,
        float damage,
        float radius,
        float critChance,
        float critMultiplier,
        ElementStatusProfile statusProfile,
        float burningDotMultiplier,
        float bleedDotMultiplier,
        float slowBonus,
        float earthBuildupPerHit
    )
    {
        if (hitbox != null)
        {
            hitbox.Damage = damage;
            hitbox.DamageType = element;
            hitbox.OneShot = true;

            hitbox.CritChance = critChance;
            hitbox.CritMultiplier = critMultiplier;

            hitbox.StatusProfile = statusProfile;
            hitbox.BurningDotMultiplier = burningDotMultiplier;
            hitbox.BleedDotMultiplier = bleedDotMultiplier;
            hitbox.SlowBonus = slowBonus;
            hitbox.EarthBuildupPerHit = earthBuildupPerHit;
        }

        ApplyRadius(radius);
        ScaleVisual(radius);
    }

    private void ApplyRadius(float radius)
    {
        if (shape == null || shape.Shape == null)
            return;

        Shape3D unique = (Shape3D)shape.Shape.Duplicate();

        if (unique is SphereShape3D sphere)
            sphere.Radius = radius;
        else
            GD.PrintErr($"ExplosionAoE: CollisionShape is not SphereShape3D (got {unique.GetType().Name}). Radius not applied.");

        shape.Shape = unique;
    }

    private void ScaleVisual(float radius)
    {
        if (visual != null)
        {
            visual.Scale = new Vector3(radius, radius, radius);
        }
    }

}
