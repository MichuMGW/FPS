using Godot;
using System;

public partial class PlayerMovement : Node
{
    [Export] public CharacterBody3D player;
    // [Export] private float _walkSpeed = 10.0f;
    // [Export] private float _sprintSpeed = 20.0f;
    // [Export] private float _jumpForce = 10.0f;
    private float _walkSpeed;
    private float _sprintSpeed;
    private float _jumpForce;

    private PlayerStats _stats;

    public override void _Process(double delta)
    {
        Move((float)delta);
        player.MoveAndSlide();

        _stats = GetNode<PlayerStats>("/root/PlayerStats");

        _walkSpeed = _stats.Speed;
        _sprintSpeed = _stats.Speed * 2;
        _stats.SpeedChanged += OnSpeedChanged;
    }

    private void OnSpeedChanged(float value){
        _walkSpeed = value;
        _sprintSpeed = value * 2;
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
        float speed = Input.IsActionPressed("Sprint") ? _sprintSpeed : _walkSpeed;
        player.Velocity = new Vector3(moveDirection.X * speed, player.Velocity.Y, moveDirection.Z * speed);

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

