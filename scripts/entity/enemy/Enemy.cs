using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
    [Export] public EnemyHealthComponent Health { get; set; }
    [Export] public EnemyMovement Movement { get; set; }
    [Export] public EnemyNavigationAgent NavAgent { get; set; }
    [Export] public StatusComponent Status { get; set; }
    [Export] public HurtboxComponent Hurtbox { get; set; }

}
