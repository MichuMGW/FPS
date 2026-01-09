using Godot;

public partial class HitInfo : Node
{
    public Node3D Source { get; set; }
    public HurtboxType HurtboxType { get; set; }
    public float DamageMultiplier { get; set; }
    public Vector3 HitPosition { get; set; }

    public float BaseDamage { get; set; }
    public Element Element { get; set; }
    public bool IsCrit;
    public float FinalDamage { get; set; }

    public ElementStatusProfile StatusProfile { get; set; }
    public float BurningDotMultiplier { get; set; } = 1f;
    public float BleedDotMultiplier { get; set; } = 1f;
    public float SlowBonus { get; set; } = 0f;
    public float EarthBuildupPerHit { get; set; } = 0f;

    public HitInfo() { }

    public HitInfo(Node3D source, HurtboxType hurtboxType, float damageMultiplier, Vector3 hitPosition)
    {
        Source = source;
        HurtboxType = hurtboxType;
        DamageMultiplier = damageMultiplier;
        HitPosition = hitPosition;
    }
}

