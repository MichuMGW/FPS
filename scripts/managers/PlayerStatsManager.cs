using Godot;
using System;

public partial class PlayerStatsManager : Node
{
    [Signal] public delegate void StatsChangedEventHandler();
    [Signal] public delegate void JumpForceChangedEventHandler(float value);
    [Signal] public delegate void MaxHealthChangedEventHandler(float value);
    [Signal] public delegate void DamageChangedEventHandler(float value);
    [Signal] public delegate void RangeChangedEventHandler(float value);
    [Signal] public delegate void SpeedChangedEventHandler(float value);
    [Signal] public delegate void RadiusChangedEventHandler(float value);
    [Signal] public delegate void SpreadChangedEventHandler(float value);

    private float _maxHealth;
    private float _range;
    private float _damage;
    private float _speed;
    private float _jumpForce;
    private float _radius;
    private float _spread;

    public float MaxHealth
    { 
        get => _maxHealth;
        set
        {
            _maxHealth = EnsureNonNegative(value);
            EmitSignal(nameof(MaxHealthChanged), value);
        }
    }

    public float Range
    {
        get => _range;
        set
        {
            _range = EnsureNonNegative(value);
            EmitSignal(nameof(RangeChanged), value);
        }
    }

    public float Damage
    {
        get => _damage;
        set
        {
            _damage = EnsureNonNegative(value);
            EmitSignal(nameof(DamageChanged), value);
        }
    }

    public float Speed
    {
        get => _speed;
        set
        {
            _speed = EnsureNonNegative(value);
            EmitSignal(nameof(SpeedChanged), value);
        }
    }

    public float Radius
    {
        get => _radius;
        set
        {
            _radius = EnsureNonNegative(value);
            EmitSignal(nameof(RadiusChanged), value);
        }
    }

    public float Spread
    {
        get => _spread;
        set
        {
            _spread = EnsureNonNegative(value);
            EmitSignal(nameof(SpreadChanged), value);
        }
    }

    public float JumpForce
    {
        get => _jumpForce;
        set
        {
            _jumpForce = EnsureNonNegative(value);
            EmitSignal(nameof(JumpForceChanged), value);
        }
    }
    
    public override void _Ready()
    {
        var resource = GD.Load<PlayerStatsResource>("res://resources/stats/PlayerStats.tres");
        LoadStatsFromResource(resource);

        //Dodać zwiększenie lvla w expManagerze
        //akcja += ApplyStatsUp
    }

    private void LoadStatsFromResource(PlayerStatsResource res)
    {
        _maxHealth = res.MaxHealth;
        _range = res.Range;
        _damage = res.Damage;
        _speed = res.Speed;
        _radius = res.Radius;
        _spread = res.Spread;
        _jumpForce = res.JumpForce;

        GD.Print($"{_maxHealth} {_speed}");
    }

    private void SetStat(StatsType statsType, float amount)
    {
        switch(statsType)
        {
            case StatsType.Damage:
                Damage = amount;
                break;
            case StatsType.Range:
                Range = amount;
                break;
            case StatsType.MaxHealth:
                MaxHealth = amount;
                break;
            case StatsType.Speed:
                Speed = amount;
                break;
            case StatsType.Radius:
                Radius = amount;
                break;
            case StatsType.Spread:
                Spread = amount;
                break;
            case StatsType.JumpForce:
                JumpForce = amount;
                break;
        }

        EmitStatsChangeSignal(statsType);
    }

    private void AddStat(StatsType statsType, float amount)
    {
        switch(statsType)
        {
            case StatsType.Damage:
                Damage += amount;
                break;
            case StatsType.Range:
                Range += amount;
                break;
            case StatsType.MaxHealth:
                MaxHealth += amount;
                break;
            case StatsType.Speed:
                Speed += amount;
                break;
            case StatsType.Radius:
                Radius += amount;
                break;
            case StatsType.Spread:
                Spread += amount;
                break;
            case StatsType.JumpForce:
                JumpForce += amount;
                break;
        }

        EmitStatsChangeSignal(statsType);
    }

    public void ApplyModifier(StatsType statsType, float modifier)
    {
        switch(statsType)
        {
            case StatsType.Damage:
                Damage *= modifier;
                break;
            case StatsType.Range:
                Range *= modifier;
                break;
            case StatsType.MaxHealth:
                MaxHealth *= modifier;
                break;
            case StatsType.Speed:
                Speed *= modifier;
                break;
            case StatsType.Radius:
                Radius *= modifier;
                break;
            case StatsType.Spread:
                Spread *= modifier;
                break;
            case StatsType.JumpForce:
                JumpForce *= modifier;
                break;
        }

        EmitStatsChangeSignal(statsType);
    }

    private void EmitStatsChangeSignal(StatsType type)
    {
        switch (type)
        {    
            case StatsType.Damage:
                EmitSignal(nameof(DamageChanged), Damage);
                break;
            case StatsType.Range:
                EmitSignal(nameof(RangeChanged), Range);
                break;
            case StatsType.MaxHealth:
                EmitSignal(nameof(MaxHealthChanged), MaxHealth);
                break;
            case StatsType.Speed:
                EmitSignal(nameof(SpeedChanged), Speed);
                break;
            case StatsType.Radius:
                EmitSignal(nameof(RadiusChanged), Radius);
                break;
            case StatsType.Spread:
                EmitSignal(nameof(SpreadChanged), Spread);
                break;
            case StatsType.JumpForce:
                EmitSignal(nameof(JumpForceChanged), Spread);
                break;
            
        }
    }

    private float EnsureNonNegative(float value)
    {
        return value > 0 ? value : 0;
    }




    
}