using Godot;
using System.Collections.Generic;

public partial class HitboxComponent : Area3D, IDamageSource
{
    [Export] public float Damage { get; set; } = 20f;
    [Export] public Element DamageType { get; set; } = Element.None;
    [Export] public float CritChance { get; set; } = 0f;
    [Export] public float CritMultiplier { get; set; } = 1.5f;

    [Export] public float RehitCooldownSeconds { get; set; } = 0f;
    [Export] public bool OneShot { get; set; } = false;

    [ExportGroup("Status")]
    [Export] public ElementStatusProfile StatusProfile { get; set; } = null;

    [ExportGroup("Status Magnitudes")]
    [Export] public float BurningDotMultiplier { get; set; } = 1f;
    [Export] public float BleedDotMultiplier { get; set; } = 1f;
    [Export] public float SlowBonus { get; set; } = 0f;
    [Export] public float EarthBuildupPerHit { get; set; } = 0f;

    private bool _active;
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            SetDeferred("monitoring", value);
            SetDeferred("monitorable", value);
        }
    }

    private readonly Dictionary<Node3D, double> _alreadyHit = new();
    private double _timeAlive;

    public override void _Ready() => Active = true;

    public override void _PhysicsProcess(double delta) => _timeAlive += delta;

    public float GetDamage() => Damage;
    public Element GetDamageType() => DamageType;
    public float GetCritChance() => CritChance;
    public float GetCritMultiplier() => CritMultiplier;
    public ElementStatusProfile GetStatusProfile() => StatusProfile;
    public float GetBurningDotMultiplier() => BurningDotMultiplier;
    public float GetBleedDotMultiplier() => BleedDotMultiplier;
    public float GetSlowBonus() => SlowBonus;
    public float GetEarthBuildupPerHit() => EarthBuildupPerHit;

    public bool CanHitAgain(Node3D target)
    {
        if (OneShot)
            return !_alreadyHit.ContainsKey(target);

        if (RehitCooldownSeconds <= 0f)
            return true;

        if (!_alreadyHit.TryGetValue(target, out var lastTime))
            return true;

        return _timeAlive - lastTime >= RehitCooldownSeconds;
    }

    public void RegisterHit(Node3D target)
    {
        _alreadyHit[target] = _timeAlive;
    }

    

}
