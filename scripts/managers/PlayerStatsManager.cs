using System.Collections.Generic;
using Godot;

public partial class PlayerStatsManager : Node
{
    private readonly Dictionary<StatId, StatValue> _stats = new();

    public float GetStat(StatId id) => _stats.TryGetValue(id, out var v) ? v.Final : 0f;

    public void SetBaseStat(StatId id, float baseValue) { /* ... */ }
    public void AddModifier(StatId id, float add, float mult) { /* ... */ }
}
