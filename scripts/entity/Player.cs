using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public HealthComponent Health {get; set;} //DODAĆ DO SCENY
    [Export] public PlayerMovement Movement {get; set;}
    [Export] public SpellCastManager SpellCastManager {get; set;}
}
