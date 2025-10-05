using Godot;

[GlobalClass]
public partial class PlayerStatsResource : Resource
{
    [Export] public float MaxHealth;
    [Export] public float Range;
    [Export] public float Damage;
    [Export] public float Speed;
    [Export] public float Radius;
    [Export] public float Spread;
}
