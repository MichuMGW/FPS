using Godot;
using System;

public static partial class PlayerStatsManager : Node
{
    [Signal] public delegate void StatsChangedEventHandler();
    [Signal] public delegate void MaxHealthChangedEventHandler(float value);
    [Signal] public delegate void ChangedEventHandler(float value);
    [Signal] public delegate void RangeChangedEventHandler(float value);
    [Signal] public delegate void DamageChangedEventHandler(float value);
    [Signal] public delegate void SpeedChangedEventHandler(float value);
    [Signal] public delegate void SpreadChangedEventHandler(float value);

    

    private float _maxHealth;
    private float _range;
    private float _damage;
    private float _speed;
    private float _radius;
    private float _spread;

    public float MaxHealth
    { 
        get => _maxHealth;
        set
        {
            _maxHealth = EnsureNonNegative(value)
            EmitSignal(nameof(MaxHealthChanged), value);
        };
    }

    public float Range
    {
        get => _range;
        set
        {
            _range = EnsureNonNegative(value)
            EmitSignal(nameof(RangeChanged), value);
        }
    }

    public float Damage
    {
        get => _damage;
        set
        {
            _damage = EnsureNonNegative(value);
            EitSignal(nameof(DamageChanged), value);
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
            radius = EnsureNonNegative(value);
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
    
    public override void _Ready()
    {
        LoadStatsFromResource();

        //Dodać zwiększenie lvla w expManagerze
        //akcja += ApplyStatsUp
    }

    private void LoadStatsFromResource(PlayerStatsResource res)
    {
        maxHealth = res.MaxHealth;
        range = res.Range;
        damage = res.damage;
        speed = res.speed;
        radius = res.radius;
        spread = res.spread;
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
            case StatsType.Health:
                Health = amount;
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
        }

        EmitSignal(nameof(StatsChanged), statsType);
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
            case StatsType.Health:
                Health += amount;
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
        }

        EmitSignal(nameof(StatsChanged), statsType);
    }

    public void ApplyModifier(StatsType statsType, float modifier)
    {
        switch(statsType)
        {
            case StatsType.Damage:
                Damage *= amount;
                break;
            case StatsType.Range:
                Range *= amount;
                break;
            case StatsType.Health:
                Health *= amount;
                break;
            case StatsType.Speed:
                Speed *= amount;
                break;
            case StatsType.Radius:
                Radius *= amount;
                break;
            case StatsType.Spread:
                Spread *= amount;
                break;
        }

        EmitSignal(nameof(StatsChanged), statsType);
    }

    private float EnsureNonNegative(float value)
    {
        return value > 0 ? value : 0;
    }




    
}