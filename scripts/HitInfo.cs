using Godot;

public partial class HitInfo : Node
{
    public Node3D Source { get; set; }
    public HurtboxType HitboxType { get; set; }
    public float DamageMultiplier { get; set; }
    public Vector3 HitPosition { get; set; }

    // --- Core damage snapshot (ważne: snapshot, nie “odczytaj później”) ---
    public float BaseDamage { get; set; }
    public Element Element { get; set; }

    // --- Status payload ---
    public ElementStatusProfile StatusProfile { get; set; } // może być null
    public float BurningDotMultiplier { get; set; } = 1f;
    public float BleedDotMultiplier { get; set; } = 1f;
    public float SlowBonus { get; set; } = 0f;
    public float EarthBuildupPerHit { get; set; } = 0f;

    public HitInfo() { }

    public HitInfo(Node3D source, HurtboxType hitboxType, float damageMultiplier, Vector3 hitPosition)
    {
        Source = source;
        HitboxType = hitboxType;
        DamageMultiplier = damageMultiplier;
        HitPosition = hitPosition;
    }
}
