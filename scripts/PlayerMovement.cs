using Godot;
using System;

public partial class PlayerMovement : Node
{
    [Export]
    public CharacterBody3D player;
    [Export]
    public Camera3D camera;

    [Export]
    float walkSpeed = 10.0f;
    float sprintSpeed = 20.0f;
    [Export]
    float jumpForce = 10.0f;

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        Move((float)delta);
        
        player.MoveAndSlide();
    }

    private void Move(float delta){
        
        Vector3 moveDirection = new Vector3();

        if(Input.IsActionPressed("MoveForward")){
            moveDirection += -player.GlobalTransform.Basis.Z;
        }
        if(Input.IsActionPressed("MoveLeft")){
            moveDirection += -player.GlobalTransform.Basis.X;
        }
        if(Input.IsActionPressed("MoveRight")){
            moveDirection += player.GlobalTransform.Basis.X;
        }
        if(Input.IsActionPressed("MoveBackward")){
            moveDirection += player.GlobalTransform.Basis.Z;
        }

        moveDirection = moveDirection.Normalized();
        float speed = Input.IsActionPressed("Sprint") ? sprintSpeed : walkSpeed;
        player.Velocity = new Vector3(moveDirection.X * speed, player.Velocity.Y, moveDirection.Z * speed);

        //Camera FOV change
        if(Input.IsActionPressed("Sprint")){
            camera.Fov = Mathf.Lerp(camera.Fov, 75, 0.1f);
        } else {
            camera.Fov = Mathf.Lerp(camera.Fov, 60, 0.1f);
        }

        if(player.IsOnFloor()){
            if(Input.IsActionPressed("Jump")){
                player.Velocity = new Vector3(player.Velocity.X, jumpForce, player.Velocity.Z);
            } else {
                player.Velocity = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
            }
        } else {
            player.Velocity += player.GetGravity() * (float)delta;
        }      
    }
}

