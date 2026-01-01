using Godot;

[GlobalClass]
public partial class DashSpellDefinition : SpellDefinition
{
    [Export] public float DashDuration = 0.5f;
    [Export] public float SpeedMultiplier = 2.0f;
    [Export] public float FovModifiedAmount = 10f;
    [Export] public float DashForce = 30f;
}
