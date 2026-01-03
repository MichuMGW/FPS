using Godot;
using System;

public partial class BossHealthComponent : HealthComponent
{
    [Signal] public delegate void BossHealthChangedEventHandler(float current, float max);
    [Signal] public delegate void BossDiedEventHandler();

    [Export] public string BossDisplayName = "Boss";

    private GameEvents _events;
    public override void _Ready()
    {
        base._Ready();

        _events = GetTree().Root.GetNode<GameEvents>("GameEvents");
        EmitSignal(nameof(BossHealthChanged), CurrentHealth, MaxHealth);
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        EmitSignal(nameof(BossHealthChanged), CurrentHealth, MaxHealth);
    }

    public override void Die()
    {
        EmitSignal(nameof(BossDied));

        base.Die();
    }
}
