using Godot;

[GlobalClass]
public partial class ProjectileSpellDefinition : SpellDefinition
{
    [Export] public PackedScene ProjectileScene { get; set; }

    [ExportGroup("Projectile Stats")]
    [Export] public float ProjectileSpeed { get; set; } = 30f;
    [Export] public float ProjectileSpread { get; set; } = 0f;
    [Export] public int ProjectilePierce { get; set; } = 0;
    [Export] public bool DieOnWorldHit { get; set; } = true;

    [ExportGroup("Scale Over Time")]
    [Export] public bool ScaleOverTime { get; set; } = false;

    [Export(PropertyHint.Range, "0.05,10,0.05")]
    public float StartScale { get; set; } = 1f;

    [Export(PropertyHint.Range, "0.05,10,0.05")]
    public float EndScale { get; set; } = 1f;

    // Czas dojścia do EndScale (0 = natychmiast)
    [Export(PropertyHint.Range, "0,10,0.05")]
    public float ScaleDurationSeconds { get; set; } = 0f;

    [ExportGroup("Explosion")]
    [Export] public bool ExplodeOnEnemyHit { get; set; } = false;

    [Export] public bool ExplodeOnWorldHit { get; set; } = false;

    // Scena VFX/AOE, instancjonowana w punkcie trafienia
    [Export] public PackedScene ExplosionScene { get; set; }

    // Jeśli explosion scene ma HitboxComponent, możesz przeskalować jej dmg
    [Export(PropertyHint.Range, "0.1,10,0.1")]
    public float ExplosionDamageMultiplier { get; set; } = 1f;

    // Jeśli explosion to tylko VFX i chcesz auto-queue-free po czasie (0 = nie ruszaj)
    [Export(PropertyHint.Range, "0,5,0.05")]
    public float ExplosionLifetimeSeconds { get; set; } = 0f;
}
