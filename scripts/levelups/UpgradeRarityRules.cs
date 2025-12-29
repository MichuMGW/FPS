public static class UpgradeRarityRules
{
    public static float Multiplier(UpgradeRarity r) => r switch
    {
        UpgradeRarity.Common => 1f,
        UpgradeRarity.Uncommon => 1.15f,
        UpgradeRarity.Epic => 1.3f,
        UpgradeRarity.Legendary => 1.5f,
        UpgradeRarity.Mythic => 2f,
        _ => 1f
    };
}