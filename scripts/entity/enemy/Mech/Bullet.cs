using Godot;
using System;

public partial class Bullet : Node3D
{
    private const float Speed = 100.0f;
    private Vector3 _velocity;
    private RayCast3D _rayCast;
    public override void _Ready()
    {
        _rayCast = GetNode<RayCast3D>("RayCast3D");
    }

    public override void _Process(double delta)
    {
        GlobalPosition += _velocity * (float)delta;
        if (_rayCast.IsColliding())
        {
            QueueFree();
        }
    }

    public void Initialize(Vector3 direction)
    {
        _velocity = direction.Normalized() * Speed;
    }
}
