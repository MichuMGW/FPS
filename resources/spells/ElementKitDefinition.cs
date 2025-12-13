using Godot;

[GlobalClass]
public partial class ElementKitDefinition : Resource
{
    [Export] public Element Element { get; set; } = Element.None;

    [Export] public SpellDefinition LeftHandSpell { get; set; }
    [Export] public SpellDefinition RightHandSpell { get; set; }
    [Export] public SpellDefinition DashSpell { get; set; }
    [Export] public SpellDefinition ShieldSpell { get; set; }
    [Export] public SpellDefinition BuffSpell { get; set; }
}
