using Godot;

[GlobalClass]
public partial class BeamSpellDefinition : ChannelSpellDefinition
{
    // opcjonalnie specyficzne parametry beama
    [Export] public float BeamWidth { get; set; } = 0.3f;
}
