using Godot;

[GlobalClass]
public partial class LevelUpUpgradeDefinition : Resource
{
    [Export] public string Id { get; set; } = "upgrade_unknown";
    [Export] public string DisplayName { get; set; } = "Upgrade";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Godot.Collections.Array<StatModDefinition> StatMods { get; set; }

    [Export] public bool CanStack { get; set; } = true;
    [Export] public int MaxStacks { get; set; } = 99;
}
