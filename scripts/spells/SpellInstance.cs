using System;
using System.Collections.Generic;

public class SpellInstance
{
    public SpellDefinition Definition { get; }
    public int Level { get; private set; }
    public float CurrentCooldown { get; private set; }

    // ew. per-spell modyfikatory:
    private readonly List<ISpellModifier> _modifiers = new();

    public SpellInstance(SpellDefinition def, int level = 1)
    {
        Definition = def;
        Level = level;
    }

    public bool CanCast => CurrentCooldown <= 0f;

    public void TickCooldown(float dt)
    {
        if (CurrentCooldown > 0f)
            CurrentCooldown = Math.Max(0f, CurrentCooldown - dt);
    }

    public void PutOnCooldown(float cdrMultiplier)
    {
        CurrentCooldown = Definition.BaseCooldown * cdrMultiplier;
    }

    public SpellCastStats BuildCastStats(PlayerStatsManager stats)
    {
        // Tutaj łączysz:
        // - base z definition
        // - staty gracza (SpellDamageMultiplier itd.)
        // - modyfikatory z itemów / unik. efektów
        var result = new SpellCastStats
        {
            Damage = Definition.BaseDamage * stats.GetStat(StatId.SpellDamageMultiplier),
            Range  = Definition.BaseRange * stats.GetStat(StatId.SpellRangeMultiplier),
            ProjectileSpeed = Definition.ProjectileSpeed * stats.GetStat(StatId.ProjectileSpeedMultiplier),
            ManaCost = Definition.BaseManaCost,
        };

        foreach (var mod in _modifiers)
            result = mod.Modify(result);

        return result;
    }
}
