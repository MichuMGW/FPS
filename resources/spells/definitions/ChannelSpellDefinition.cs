using Godot;

[GlobalClass]
public abstract partial class ChannelSpellDefinition : SpellDefinition
{
    [Export] public PackedScene EmitterScene { get; set; }

    [Export] public float TickRate { get; set; } = 0.1f;
    [Export] public float BaseDuration { get; set; } = 3f;
}
