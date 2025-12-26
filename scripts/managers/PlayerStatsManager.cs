using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerStatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(int statId, float newValue, float oldValue);
    [Signal] public delegate void StatsChangedEventHandler();

    private readonly Dictionary<StatId, StatValue> _stats = new();

    public float GetStat(StatId id) => _stats.TryGetValue(id, out var v) ? v.Final : GetDefault(id);

    private float GetDefault(StatId id)
    {
        // sensowne defaulty, żeby nie było 0 jak ktoś zapomni ustawić
        return id switch
        {
            StatId.DamageMultiplier => 1f,
            StatId.RangeMultiplier => 1f,
            StatId.ProjectileSpeedMultiplier => 1f,
            StatId.CooldownReduction => 0f,
            _ => 0f
        };
    }

    private StatValue GetOrCreate(StatId id)
    {
        if (_stats.TryGetValue(id, out var v))
            return v;

        // domyślne Multiplier = 1
        v = new StatValue { Base = 0f, Additive = 0f, Multiplier = 1f };
        _stats[id] = v;
        return v;
    }

    public void SetBaseStat(StatId id, float baseValue)
    {
        float old = GetStat(id);
        var v = GetOrCreate(id);

        v.Base = baseValue;
        _stats[id] = v;

        EmitIfChanged(id, old);
    }

    public void AddModifier(StatId id, float add = 0f, float mult = 1f)
    {
        float old = GetStat(id);
        var v = GetOrCreate(id);

        v.Additive += add;
        v.Multiplier *= mult;
        if (v.Multiplier < 0f) v.Multiplier = 0f;

        _stats[id] = v;

        EmitIfChanged(id, old);
    }

    public void ResetAll()
    {
        _stats.Clear();
        EmitSignal(SignalName.StatsChanged);
    }

    public void InitializeFromResource(PlayerStatsResource res, bool clearFirst = true)
    {
        if (res == null)
        {
            GD.PrintErr("PlayerStatsManager: InitializeFromResource called with null resource.");
            return;
        }

        if (clearFirst)
            _stats.Clear();

        // BAZY (Base)
        SetBaseStat(StatId.MaxHealth, res.MaxHealth);
        SetBaseStat(StatId.MoveSpeed, res.MoveSpeed);
        SetBaseStat(StatId.JumpForce, res.JumpForce);
        SetBaseStat(StatId.JumpCount, res.JumpCount);

        // MULTIPLIERY – jako base, bo to wartości wyjściowe
        SetBaseStat(StatId.DamageMultiplier, res.SpellDamageMultiplier);
        SetBaseStat(StatId.RangeMultiplier, res.SpellRangeMultiplier);
        SetBaseStat(StatId.ProjectileSpeedMultiplier, res.ProjectileSpeedMultiplier);

        // CooldownReduction – base (0..1)
        SetBaseStat(StatId.CooldownReduction, res.CooldownReduction);

        EmitSignal(SignalName.StatsChanged);
    }

    private void EmitIfChanged(StatId id, float oldFinal)
    {
        float now = GetStat(id);
        if (Mathf.IsEqualApprox(oldFinal, now))
            return;

        EmitSignal(SignalName.StatChanged, (int)id, now, oldFinal);
        EmitSignal(SignalName.StatsChanged);
    }
}
