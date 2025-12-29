using Godot;

public partial class LevelUpOffer : Resource
{
    [Export] public LevelUpUpgradeDefinition Upgrade;
    [Export] public UpgradeRarity Rarity;

    public float RarityMult => UpgradeRarityRules.Multiplier(Rarity);
}
