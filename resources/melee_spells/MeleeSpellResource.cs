using Godot;

[GlobalClass]
public partial class MeleeSpellResource : SpellResource {
    [Export] PackedScene ProjectileScene {get; set;}
}