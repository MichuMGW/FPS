using Godot;

[GlobalClass]
public abstract partial class SpellDefinition : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string DisplayName { get; set; }

    [Export] public Element Element { get; set; }

    [Export] public SpellCastMode CastMode { get; set; }
    [Export] public SpellBehaviourType BehaviourType { get; set; }

    [ExportGroup("Core Stats")]
    [Export] public float BaseDamage { get; set; } = 10f;
    [Export] public float BaseRange { get; set; } = 20f;
    [Export] public float BaseCooldown { get; set; } = 1f;
    [Export] public float BaseManaCost { get; set; } = 0f;

    [ExportGroup("Rehit (DoT)")]
    [Export] public bool EnableRehit { get; set; } = false;
    [Export(PropertyHint.Range, "0.01,2.0,0.01")]
    public float RehitIntervalSeconds { get; set; } = 0.25f;
    // Jak długo ma działać rehit po “zaczepieniu” (0 = bez limitu, do końca życia pocisku)
    [Export(PropertyHint.Range, "0,10,0.1")]
    public float RehitMaxDurationSeconds { get; set; } = 0f;

    [ExportGroup("Status Magnitudes")]
    [Export] public float BurningDotMultiplier = 1f; // FireExplosion np. 4.0
    [Export] public float SlowMultiplierBonus = 0f;  // jeśli chcesz różne slowy
    [Export] public float BleedDotMultiplier = 1f;

    [Export] public float EarthBuildupPerHit = 0f;   // Earth spell “siła”


}
