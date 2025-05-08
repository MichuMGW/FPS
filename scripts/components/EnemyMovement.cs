using Godot;
using System;

public partial class EnemyMovement : Node
{
    [Export] public Enemy enemy;
    public CharacterBody3D player;
    [Export] public EnemyNavigationAgent navAgent;
    private float baseSpeed = 5.0f;
    private float currentSpeed;
    private float gravity = -9.8f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        currentSpeed = baseSpeed;

        enemy.Status.SlowStarted += OnSlowStarted;
        enemy.Status.SlowEnded += OnSlowEnded;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 newVelocity = navAgent.CalculatePath(player, enemy);
        enemy.Velocity = newVelocity * currentSpeed;
        // Apply gravity
        if (!enemy.IsOnFloor()){
            enemy.Velocity += new Vector3(enemy.Velocity.X, gravity * (float)delta, enemy.Velocity.Z);
        }
        else{
            enemy.Velocity = new Vector3(enemy.Velocity.X, 0, enemy.Velocity.Z);
        }
        enemy.MoveAndSlide();
    }

    private void OnSlowStarted(float slowAmount){
        currentSpeed = baseSpeed * slowAmount;
    }

    private void OnSlowEnded(){
        GD.Print("SLOW ENDED");
        currentSpeed = baseSpeed;
    }


}
