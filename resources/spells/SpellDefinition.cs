using Godot;
using System;

[GlobalClass]
public partial class SpellDefinition : Resource
{
    [Export] public string Id { get; set; }  // "fireball"
    [Export] public string DisplayName { get; set; }

    [Export] public float BaseDamage { get; set; }
    [Export] public float BaseRange { get; set; }
    [Export] public float BaseCooldown { get; set; }
    [Export] public float BaseManaCost { get; set; }

    [Export] public SpellCastMode CastMode { get; set; }
    [Export] public SpellBehaviourType BehaviourType { get; set; }
    [Export] public PackedScene ProjectileScene { get; set; } // null dla melee/buffów
    [Export] public Element Element { get; set; }

    [Export] public float ProjectileSpeed { get; set; }
    [Export] public float ProjectileSpread { get; set; }
}
