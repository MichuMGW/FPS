using Godot;
using System;
using System.Collections.Generic;

public partial class ItemInventory : Node
{
    [Signal] public delegate void InventoryChangedEventHandler();

    private readonly Dictionary<string, ItemStack> _stacks = new();

    [Export] public NodePath PlayerStatsManagerPath;

    private PlayerStatsManager _stats;
    private GameEvents _events;

    private sealed class ItemStack
    {
        public ItemDefinition Def;
        public int Count;
    }

    public override void _Ready()
    {
        _stats = GetTree().GetFirstNodeInGroup("player_stats_manager") as PlayerStatsManager;
        if (_stats == null)
            GD.PushWarning("RunInventory: missing PlayerStatsManager reference.");

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        if (_events != null)
            _events.ChestRewardClaimed += OnChestRewardClaimed;
        // na start runa: pusto
        ClearRun();
    }

    private void OnChestRewardClaimed(ItemDefinition item)
    {
        AddItem(item);
    }

    public IReadOnlyDictionary<string, int> GetCountsByItemId()
    {
        var dict = new Dictionary<string, int>(_stacks.Count);
        foreach (var kv in _stacks)
            dict[kv.Key] = kv.Value.Count;
        return dict;
    }

    public IEnumerable<(ItemDefinition def, int count)> GetStacks()
    {
        foreach (var s in _stacks.Values)
            yield return (s.Def, s.Count);
    }

    public bool AddItem(ItemDefinition item)
    {
        if (item == null) return false;
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            GD.PrintErr("RunInventory.AddItem: item has empty Id.");
            return false;
        }

        if (!_stacks.TryGetValue(item.Id, out var stack))
        {
            stack = new ItemStack { Def = item, Count = 0 };
            _stacks[item.Id] = stack;
        }

        stack.Count += 1;

        RebuildStatSources();
        GD.Print($"RunInventory: added item {item.Id}, new count: {stack.Count}");
        EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    public void ClearRun()
    {
        _stacks.Clear();
        RebuildStatSources();
        EmitSignal(SignalName.InventoryChanged);
    }

    private void RebuildStatSources()
    {
        if (_stats == null)
            return;

        // Usuń poprzednie źródła itemowe i ustaw od nowa.
        // Najprościej: trzymasz prefix.
        _stats.ClearAllSources();

        foreach (var stack in _stacks.Values)
        {
            var item = stack.Def;
            int count = stack.Count;
            if (count <= 0) continue;

            string sourceId = $"item:{item.Id}";

            // ItemDefinition ma listę StatModDefinition
            foreach (var m in item.StatModifier)
            {
                // stackowanie: Add sumuje się liniowo, Mult możesz np. potęgować
                // multTotal = mult^count (dla roguelite to standard)
                float addTotal = m.Add * count;
                float multTotal = PowSafe(m.Mult, count);

                _stats.SetModifier(sourceId, m.Stat, addTotal, multTotal);
            }
        }
    }

    private float PowSafe(float mult, int count)
    {
        if (count <= 0) return 1f;
        if (Mathf.IsEqualApprox(mult, 1f)) return 1f;
        if (mult <= 0f) return 0f;

        float r = 1f;
        for (int i = 0; i < count; i++)
            r *= mult;
        return r;
    }
}
