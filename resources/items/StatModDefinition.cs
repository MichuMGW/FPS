using Godot;

[GlobalClass]
public partial class StatModDefinition : Resource
{
    [Export] public StatId Stat;
    [Export] public float Add = 0f;
    [Export] public float Mult = 1f;
}