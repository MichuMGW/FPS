using Godot;
using System;

public partial class EnemyNavigationAgent : NavigationAgent3D
{
    public Vector3 CalculatePath(CharacterBody3D player, CharacterBody3D enemy)
    {
        if (player == null)
        {
            GD.Print("Player not found");
            return Vector3.Zero;
        }
        TargetPosition = player.GlobalPosition;
        Vector3 nextPathPosition = GetNextPathPosition();
        Vector3 newDirection = (nextPathPosition - enemy.GlobalPosition).Normalized();
        return newDirection;
    }
}
