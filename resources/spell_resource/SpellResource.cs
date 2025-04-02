using Godot;

[GlobalClass]
public partial class SpellResource : Resource
{
	[Export] public string SpellName {get; set;}
    [Export] public float SpellDamage {get; set;}
    [Export] public float SpellCooldown {get; set;}
    [Export] public float SpellRange {get; set;}
    [Export] public float ManaCost {get; set;}
    [Export] public PackedScene SpellScene {get; set;}
}
