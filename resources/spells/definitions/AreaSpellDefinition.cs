using Godot;

[GlobalClass]
public partial class AreaSpellDefinition : ChannelSpellDefinition
{
    [Export] public float AuraRadius { get; set; } = 2.5f;
}
