using Godot;
using System;
using System.Collections.Generic;

public partial class LevelUpManager : Node
{
    [Export] public Godot.Collections.Array<LevelUpUpgradeDefinition> UpgradePool = new();
    [Export] public int OptionsPerLevelUp = 3;

    // ile razy wybrano dany upgrade (nie uwzględnia rarity)
    private readonly Dictionary<string, int> _stacks = new();

    private PlayerStatsManager _stats;
    private GameEvents _events;
    private ExperienceManager _exp;

    private readonly RandomNumberGenerator _rng = new();
    private int _pickIndex = 0;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _exp = GetTree().CurrentScene.GetNodeOrNull<ExperienceManager>("ExperienceManager");
        _stats = GetTree().GetFirstNodeInGroup("player_stats_manager") as PlayerStatsManager;

        if (_events == null) GD.PushError("[LevelUpManager] Missing GameEvents autoload.");
        if (_exp == null) GD.PushWarning("[LevelUpManager] Missing ExperienceManager (group: experience_manager).");
        if (_stats == null) GD.PushWarning("[LevelUpManager] Missing PlayerStatsManager (group: player_stats_manager).");

        if (_exp != null)
            _exp.LevelUp += OnLevelUp;

        if (_events != null)
            _events.LevelUpChoiceResolved += OnChoiceResolved;
    }

    public override void _ExitTree()
    {
        if (_exp != null)
            _exp.LevelUp -= OnLevelUp;

        if (_events != null)
            _events.LevelUpChoiceResolved -= OnChoiceResolved;
    }

    public void ResetRun()
    {
        _stacks.Clear();
        _pickIndex = 0;

        // Czyścimy tylko picki levelupów (wymaga ClearSourcesByPrefix w PlayerStatsManager)
        _stats?.ClearSourcesByPrefix("lvlpick:");
    }

    private void OnLevelUp(int newLevel)
    {
        if (_events == null) return;

        var options = RollOptions(OptionsPerLevelUp);
        if (options == null || options.Count == 0) return;

        _events.RequestLevelUpChoice(options);
    }

    private void OnChoiceResolved(LevelUpOffer picked)
    {
        if (picked == null || picked.Upgrade == null) return;

        // tracking “stacks”
        AddStack(picked.Upgrade);

        // aplikuj staty z rarity jako osobny sourceId (żeby mythic faktycznie był mythic)
        ApplyOfferOnce(picked);
    }

    private void AddStack(LevelUpUpgradeDefinition up)
    {
        if (up == null || string.IsNullOrWhiteSpace(up.Id)) return;

        int current = _stacks.TryGetValue(up.Id, out var c) ? c : 0;

        if (!up.CanStack && current > 0)
            return;

        if (up.CanStack && current >= Mathf.Max(1, up.MaxStacks))
            return;

        _stacks[up.Id] = current + 1;
    }

    private void ApplyOfferOnce(LevelUpOffer offer)
    {
        if (_stats == null) return;

        float rm = offer.RarityMult;
        string upId = offer.Upgrade.Id;

        // unikalne źródło per wybór (a nie per upgradeId)
        string sourceId = $"lvlpick:{_pickIndex++}:{upId}:{offer.Rarity}";

        var mods = offer.Upgrade.StatMods;
        if (mods == null || mods.Count == 0)
            return;

        foreach (var m in mods)
        {
            float add = m.Add * rm;
            float mult = ScaleMult(m.Mult, rm);

            _stats.SetModifier(sourceId, m.Stat, add, mult);
        }
    }

    private Godot.Collections.Array<LevelUpOffer> RollOptions(int count)
    {
        var result = new Godot.Collections.Array<LevelUpOffer>();
        if (UpgradePool == null || UpgradePool.Count == 0) return result;

        // kandydaci: tylko ci co jeszcze mogą wypaść (stack limit)
        var candidates = new List<LevelUpUpgradeDefinition>();
        foreach (var u in UpgradePool)
        {
            if (u == null || string.IsNullOrWhiteSpace(u.Id)) continue;

            int stacks = _stacks.TryGetValue(u.Id, out var c) ? c : 0;

            if (!u.CanStack && stacks > 0) continue;
            if (u.CanStack && stacks >= Mathf.Max(1, u.MaxStacks)) continue;

            candidates.Add(u);
        }

        while (result.Count < count && candidates.Count > 0)
        {
            int idx = _rng.RandiRange(0, candidates.Count - 1);
            var up = candidates[idx];
            candidates.RemoveAt(idx);

            var offer = new LevelUpOffer
            {
                Upgrade = up,
                Rarity = RollRarity()
            };

            result.Add(offer);
        }

        return result;
    }

    private UpgradeRarity RollRarity()
    {
        // proste wagi, zmienisz sobie jak przestaniesz “na oko”
        float roll = _rng.Randf() * 100f;

        if (roll < 70f) return UpgradeRarity.Common;
        if (roll < 90f) return UpgradeRarity.Uncommon;
        if (roll < 98f) return UpgradeRarity.Epic;
        if (roll < 99.5f) return UpgradeRarity.Legendary;
        return UpgradeRarity.Mythic;
    }

    private float ScaleMult(float baseMult, float rarityMult)
        => 1f + (baseMult - 1f) * rarityMult;
}
