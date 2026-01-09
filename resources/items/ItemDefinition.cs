using Godot;

[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string Id = "item_unknown";
    [Export] public string DisplayName = "Item";
    [Export(PropertyHint.MultilineText)] public string Description = "Description";
    [Export] public Texture2D Icon;
    [Export] public PackedScene PreviewScene;
    [Export] public ItemRarity Rarity = ItemRarity.Common;

    // USUNIĘTO: { get; set; } oraz = new()
    [Export] public Godot.Collections.Array<StatModDefinition> StatModifier;
}