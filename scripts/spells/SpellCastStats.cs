public struct SpellCastStats
{
    public float Damage;
    public float Range;
    public float ProjectileSpeed;
    public float ManaCost;
    public float Radius;
    public float PierceCount;
    public float CritChance;
    public float CritMultiplier;
    public float Duration;

    // Rehit (z SpellDefinition, modyfikowalne)
    public bool EnableRehit;
    public float RehitIntervalSeconds;

    // Projectile features (modyfikowalne)
    public bool DieOnWorldHit;

    public bool ScaleOverTime;
    public float StartScale;
    public float EndScale;
    public float ScaleDurationSeconds;

    public bool ExplodeOnEnemyHit;
    public bool ExplodeOnWorldHit;
    public float ExplosionDamageMultiplier;
    public float ExplosionLifetimeSeconds;
}
