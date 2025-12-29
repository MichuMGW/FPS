using Godot;

public partial class ChestRewardContext : RefCounted
{
    public Chest Chest { get; }
    public ItemDefinition Item { get; }
    public int Cost { get; }

    public ChestRewardContext(Chest chest, ItemDefinition item, int cost)
    {
        Chest = chest;
        Item = item;
        Cost = cost;
    }
}
