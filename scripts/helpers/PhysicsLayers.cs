using Godot;
using System;

public partial class PhysicsLayers : Node
{
    public const uint TERRAIN = 1 << 0;
    public const uint PLAYER_BODY = 1 << 1;
    public const uint PLAYER_HURTBOX = 1 << 2;
    public const uint PLAYER_HITBOX = 1 << 3;
    public const uint ENEMY_BODY = 1 << 4;
    public const uint ENEMY_HURTBOX = 1 << 5; 
    public const uint ENEMY_HITBOX = 1 << 6;
    public const uint PICKUPS = 1 << 7;
}
