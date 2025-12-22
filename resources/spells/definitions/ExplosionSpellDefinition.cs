using Godot;

[GlobalClass]
public partial class ExplosionSpellDefinition : SpellDefinition
{
    [Export] public PackedScene IndicatorScene { get; set; }
    [Export] public PackedScene ExplosionScene { get; set; }

    [Export] public float ChargeTime { get; set; } = 0.75f;

    [Export] public float StartRadius { get; set; } = 1f;
    [Export] public float MaxRadius { get; set; } = 5f;
    [Export] public float GrowthRate { get; set; } = 3f;

    [Export] public float MaxPlacementDistance { get; set; } = 25f;
    [Export] public uint PlacementMask { get; set; } = PhysicsLayers.TERRAIN | PhysicsLayers.ENEMY_BODY;
}
