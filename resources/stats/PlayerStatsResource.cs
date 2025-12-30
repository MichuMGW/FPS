using Godot;

[GlobalClass]
public partial class PlayerStatsResource : Resource
{
    [Export] public float MaxHealth = 100f;
    [Export] public float HealthRegen = 0f; // HP na sekundę
    [Export] public float MoveSpeed = 5f;
    [Export] public float JumpForce = 10f;
    [Export] public float BaseDamage = 10f;
    [Export] public float CritChance = 0f;        // 0.15 = 15%
    [Export] public float CritMultiplier = 1.5f;  // x1

    // Redukcja cooldownów – domyślnie 0 (czyli brak redukcji)
    [Export] public float CooldownReduction = 0f;
    [Export] public int JumpCount = 1;

    [Export] public float KnockbackResistance = 0f;   // 0.0 = brak, 1.0 = full immune
    [Export] public float KnockbackMultiplier = 1f;
}
