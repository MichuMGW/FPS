using Godot;

[GlobalClass]
public partial class ProjectileSpellDefinition : SpellDefinition
{
    [Export] public PackedScene ProjectileScene { get; set; }

    [Export] public float ProjectileSpeed { get; set; } = 30f;
    [Export] public float ProjectileSpread { get; set; } = 0f;
}
