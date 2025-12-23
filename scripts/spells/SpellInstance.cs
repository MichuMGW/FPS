using System;
using System.Collections.Generic;

public class SpellInstance
{
    public SpellDefinition Definition { get; }
    public int Level { get; private set; }
    public float CurrentCooldown { get; private set; }

    private readonly List<ISpellModifier> _modifiers = new();

    public SpellInstance(SpellDefinition def, int level = 1)
    {
        Definition = def;
        Level = level;
    }

    public bool CanCast => CurrentCooldown <= 0f;

    public void AddModifier(ISpellModifier modifier) => _modifiers.Add(modifier);
    public void RemoveModifier(string id) => _modifiers.RemoveAll(m => m.Id == id);

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
        var def = Definition;

        SpellCastStats result = new SpellCastStats
        {
            Damage = def.BaseDamage * stats.GetStat(StatId.SpellDamageMultiplier),
            Range = def.BaseRange * stats.GetStat(StatId.SpellRangeMultiplier),
            ManaCost = def.BaseManaCost,

            ProjectileSpeed = 0f,
            Radius = 0f,
            Duration = 0f,
            CritChance = 0f,
            CritMultiplier = 0f,
            PierceCount = 0f
        };

        result.EnableRehit = def.EnableRehit;
        result.RehitIntervalSeconds = def.RehitIntervalSeconds;

        // Typowe “dodatkowe staty” zależne od typu definicji
        if (def is ProjectileSpellDefinition proj)
        {
            result.ProjectileSpeed = proj.ProjectileSpeed * stats.GetStat(StatId.ProjectileSpeedMultiplier);
            result.PierceCount = proj.ProjectilePierce + stats.GetStat(StatId.ProjectilePierce);

            result.DieOnWorldHit = proj.DieOnWorldHit;

            result.ScaleOverTime = proj.ScaleOverTime;
            result.StartScale = proj.StartScale;
            result.EndScale = proj.EndScale;
            result.ScaleDurationSeconds = proj.ScaleDurationSeconds;

            result.ExplodeOnEnemyHit = proj.ExplodeOnEnemyHit;
            result.ExplodeOnWorldHit = proj.ExplodeOnWorldHit;
            result.ExplosionDamageMultiplier = proj.ExplosionDamageMultiplier;
            result.ExplosionLifetimeSeconds = proj.ExplosionLifetimeSeconds;
        }
        else if (def is DashSpellDefinition dash)
        {
            // możesz wykorzystać ProjectileSpeed jako “speed” w stats, żeby nie mnożyć pól
            result.ProjectileSpeed = dash.DashSpeed;
        }
        else if (def is AreaSpellDefinition area)
        {
            result.Radius = area.AuraRadius;
            result.Duration = area.BaseDuration;
        }
        else if (def is BeamSpellDefinition beam)
        {
            result.Duration = beam.BaseDuration;
        }
        else if (def is ExplosionSpellDefinition)
        {
            // radius/duration rosną w trakcie hold, więc tu nic nie musisz
        }

        foreach (var mod in _modifiers)
            result = mod.Modify(result, this, stats);

        return result;
    }
}
