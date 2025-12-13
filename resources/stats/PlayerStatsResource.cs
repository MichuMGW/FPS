using Godot;

[GlobalClass]
public partial class PlayerStatsResource : Resource
{
    [Export] public float MaxHealth = 100f;
    [Export] public float MoveSpeed = 8f;
    [Export] public float JumpForce = 10f;

    // Multipliery – domyślnie 1
    [Export] public float SpellDamageMultiplier = 1f;
    [Export] public float SpellRangeMultiplier = 1f;
    [Export] public float ProjectileSpeedMultiplier = 1f;

    // Redukcja cooldownów – domyślnie 0 (czyli brak redukcji)
    [Export] public float CooldownReduction = 0f;
    [Export] public int JumpCount = 1;

    [Export] public float KnockbackResistance = 0f;   // 0.0 = brak, 1.0 = full immune
    [Export] public float KnockbackMultiplier = 1f;
}
