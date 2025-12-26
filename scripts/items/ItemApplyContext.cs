using Godot;

public struct ItemApplyContext
{
    public Node3D Player;
    public PlayerStatsManager Stats;
    public PlayerSpellController Spells;
    public string SourceId; // np. item.Id albo item.DisplayName
}
