using Godot;
using System;

public partial class Bullet : Node3D
{
    private const float Speed = 100.0f;
    private const float MaxLifeTime = 2f;
    private float _lifeTime;
    private Vector3 _velocity;
    private RayCast3D _rayCast;
    public override void _Ready()
    {
        _rayCast = GetNode<RayCast3D>("RayCast3D");
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        _lifeTime += dt;

        if (_lifeTime >= MaxLifeTime)
        {
            QueueFree();
        }

        GlobalPosition += _velocity * dt;
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
