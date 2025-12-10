public interface ISpellModifier
{
    /// <summary>
    /// Identyfikator modyfikatora (do debugowania / usuwania).
    /// Np. "fire_upgrade_lv2", "ring_of_power", "buff_haste".
    /// </summary>
    string Id { get; }
    int Priority { get; }
    bool IsExpired { get; }

    SpellCastStats Modify(SpellCastStats stats, SpellInstance instance, PlayerStatsManager playerStats);
}
