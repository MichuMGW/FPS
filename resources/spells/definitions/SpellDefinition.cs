using Godot;

[GlobalClass]
public abstract partial class SpellDefinition : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string DisplayName { get; set; }

    [Export] public Element Element { get; set; }

    [Export] public SpellCastMode CastMode { get; set; }
    [Export] public SpellBehaviourType BehaviourType { get; set; }

    [ExportGroup("Core Stats")]
    [Export] public float BaseDamage { get; set; } = 10f;
    [Export] public float BaseRange { get; set; } = 20f;
    [Export] public float BaseCooldown { get; set; } = 1f;
    [Export] public float BaseManaCost { get; set; } = 0f;
}
