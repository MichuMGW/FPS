using Godot;
using System;

public partial class EnemyMovement : Node
{
    [Export] public CharacterBody3D enemy;
    public CharacterBody3D player;
    [Export] public EnemyNavigationAgent navAgent;
    public float speed = 5.0f;
    public float gravity = -9.8f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
    }

    public override void _Process(double delta)
    {
        Vector3 newVelocity = navAgent.CalculatePath(player, enemy);
        enemy.Velocity = newVelocity * speed;
        // Apply gravity
        if (!enemy.IsOnFloor()){
            enemy.Velocity += new Vector3(enemy.Velocity.X, gravity * (float)delta, enemy.Velocity.Z);
        }
        else{
            enemy.Velocity = new Vector3(enemy.Velocity.X, 0, enemy.Velocity.Z);
        }
        enemy.MoveAndSlide();
    }

}
