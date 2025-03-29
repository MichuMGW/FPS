using Godot;

[GlobalClass]
public partial class ProjectileSpellResource : SpellResource
{
    [Export] public float ProjectileSpeed {get; set;}
    [Export] public float ProjectileSpread {get; set;}
    [Export] public float ProjectileRange {get; set;}
}