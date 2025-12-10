using Godot;
using System;

public partial class PlayerHud : Control
{
    private Player _player;
    private ProgressBar _healthBar;
    private ProgressBar _manaBar;
    private ProgressBar _expBar;

    public override void _Ready()
    {
        FindNodes();

        _player.Health.CurrentHealthChanged += OnCurrentHealthChanged;
        _player.PlayerStats.MaxHealthChanged += OnMaxHealthChanged;
    }

    private void OnMaxHealthChanged(float value)
    {
        _healthBar.MaxValue = _player.Health.MaxHealth;
    }


    private void OnCurrentHealthChanged()
    {
        _healthBar.Value = _player.Health.CurrentHealth;
    }



    private void FindNodes()
    {
        _healthBar = GetNode<ProgressBar>("VBoxContainer/HealthBar");
        _manaBar = GetNode<ProgressBar>("VBoxContainer/ManaBar");
        _expBar = GetNode<ProgressBar>("VBoxContainer/ExpBar");

        _player = GetTree().GetFirstNodeInGroup("player") as Player;
    }

}
