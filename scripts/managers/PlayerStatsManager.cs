using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerStatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(int statId, float newValue, float oldValue);
    [Signal] public delegate void StatsChangedEventHandler();

    // Base staty
    private readonly Dictionary<StatId, float> _base = new();

    private readonly Dictionary<string, Dictionary<StatId, StatContribution>> _sources = new();

    // wynik końcowy
    private readonly Dictionary<StatId, float> _final = new();

    private struct StatContribution
    {
        public float Add;
        public float Mult;
    }

    public float GetStat(StatId id)
        => _final.TryGetValue(id, out var v) ? v : GetDefault(id);

    public void SetBaseStat(StatId id, float baseValue)
    {
        _base[id] = baseValue;
        RecomputeAll();
    }

    /// <summary>
    /// Ustaw wkład źródła (item/buff) dla konkretnego statId.
    /// Nadpisuje wcześniejszy wkład tego samego źródła.
    /// </summary>
    public void SetModifier(string sourceId, StatId stat, float add, float mult)
    {
        if (!_sources.TryGetValue(sourceId, out var dict))
        {
            dict = new Dictionary<StatId, StatContribution>();
            _sources[sourceId] = dict;
        }

        dict[stat] = new StatContribution { Add = add, Mult = mult };
        RecomputeAll();
    }

    public void RemoveSource(string sourceId)
    {
        if (_sources.Remove(sourceId))
            RecomputeAll();
    }

    public void ClearAllSources()
    {
        _sources.Clear();
        RecomputeAll();
    }

    public void ClearSourcesByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return;

        var toRemove = new List<string>();
        foreach (var key in _sources.Keys)
            if (key.StartsWith(prefix, StringComparison.Ordinal))
                toRemove.Add(key);

        if (toRemove.Count == 0) return;

        foreach (var k in toRemove)
            _sources.Remove(k);

        RecomputeAll();
    }

    public void InitializeFromResource(PlayerStatsResource res, bool clearFirst = true)
    {
        if (res == null)
        {
            GD.PrintErr("PlayerStatsManager: InitializeFromResource called with null.");
            return;
        }

        if (clearFirst)
        {
            _base.Clear();
            _sources.Clear();
            _final.Clear();
        }

        // Base
        _base[StatId.MaxHealth] = res.MaxHealth;
        _base[StatId.HealthRegen] = res.HealthRegen;
        _base[StatId.MoveSpeed] = res.MoveSpeed;
        _base[StatId.JumpForce] = res.JumpForce;
        _base[StatId.JumpCount] = res.JumpCount;
        _base[StatId.BaseDamage] = res.BaseDamage;
        _base[StatId.CritChance] = res.CritChance;
        _base[StatId.CritMultiplier] = res.CritMultiplier;

        _base[StatId.CooldownReduction] = res.CooldownReduction;

        RecomputeAll();
    }

    private void RecomputeAll()
    {
        // Zbierz wszystkie staty, które mogą istnieć: base + sources
        var touched = new HashSet<StatId>();
        foreach (var k in _base.Keys) touched.Add(k);
        foreach (var s in _sources.Values)
            foreach (var k in s.Keys)
                touched.Add(k);

        bool anyChanged = false;

        foreach (var id in touched)
        {
            float old = _final.TryGetValue(id, out var oldV) ? oldV : GetDefault(id);

            float baseV = _base.TryGetValue(id, out var b) ? b : GetDefaultBase(id);

            float addSum = 0f;
            float multProd = 1f;

            foreach (var src in _sources.Values)
            {
                if (!src.TryGetValue(id, out var c))
                    continue;

                addSum += c.Add;
                multProd *= (c.Mult <= 0f ? 0f : c.Mult);
            }

            float now = (baseV + addSum) * multProd;

            _final[id] = now;

            if (!Mathf.IsEqualApprox(old, now))
            {
                anyChanged = true;
                EmitSignal(SignalName.StatChanged, (int)id, now, old);
            }
        }

        if (anyChanged)
            EmitSignal(SignalName.StatsChanged);
    }

    private float GetDefault(StatId id)
    {
        return id switch
        {
            StatId.BaseDamage => 10f,

            StatId.DamageMultiplier => 1f,
            StatId.RangeMultiplier => 1f,
            StatId.ProjectileSpeedMultiplier => 1f,

            StatId.CritChance => 0f,
            StatId.CritMultiplier => 1.5f,

            StatId.ManaCostMultiplier => 1f,
            StatId.RehitIntervalMultiplier => 1f,
            StatId.KnockbackMultiplier => 1f,

            StatId.CooldownReduction => 0f,

            _ => 0f
        };
    }

    private float GetDefaultBase(StatId id)
    {
        // baza, gdy nie ustawiono base
        return id switch
        {
            StatId.BaseDamage => 10f,
            StatId.DamageMultiplier => 1f,
            StatId.RangeMultiplier => 1f,
            StatId.ProjectileSpeedMultiplier => 1f,
            StatId.CritMultiplier => 1.5f,
            StatId.ManaCostMultiplier => 1f,
            StatId.RehitIntervalMultiplier => 1f,
            StatId.KnockbackMultiplier => 1f,
            _ => 0f
        };
    }
}
