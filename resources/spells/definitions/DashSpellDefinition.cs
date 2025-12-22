using Godot;

[GlobalClass]
public partial class DashSpellDefinition : SpellDefinition
{
    [Export] public float DashSpeed { get; set; } = 18f;
    [Export] public bool KeepYVelocity { get; set; } = true;
}
