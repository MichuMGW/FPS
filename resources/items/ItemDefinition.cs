using Godot;

[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string Id { get; set; } = "item_unknown";
    [Export] public string DisplayName { get; set; } = "Item";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Description";
    [Export] public Texture2D Icon { get; set; } // pod UI
    [Export] public PackedScene PreviewScene { get; set; }
    [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [Export] public Godot.Collections.Array<StatModDefinition> StatModifier { get; set; } = new();
}

