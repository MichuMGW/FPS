using Godot;

[GlobalClass]
public partial class EnemyStatsResource : Resource
{
    [Export] public float BaseMoveSpeed;
    [Export] public float BaseMaxHealth;
    [Export] public float BaseDamage;
}