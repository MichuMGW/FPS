using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public PlayerStatsManager PlayerStats { get; set; }
    [Export] public PlayerHealthComponent Health {get; set;} //DODAĆ DO SCENY
    [Export] public PlayerMovement Movement {get; set;}
    [Export] public SpellCastManager SpellCastManager {get; set;}
    [Export] public Node3D AimTarget {get; private set;}
    [Export] public KnockbackComponent Knockback { get; private set; }

    public override void _PhysicsProcess(double delta)
    {

        Knockback.PhysicsUpdate(delta);

        MoveAndSlide();
    }
}
