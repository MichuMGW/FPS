using Godot;
using System;

public partial class ArrowProjectile : Node3D
{
    [Export] public float Speed { get; set; } = 60f;
    [Export] public float Gravity { get; set; } = 30f;
    [Export] public float LifeTime { get; set; } = 5f;
    [Export] public float GravityDelay = 0.2f;

    private float _time = 0f;

    public Vector3 Velocity;

    private float _timeAlive = 0f;

    public override void _Ready()
    {
        // GetNode<GpuParticles3D>("GPUParticles3D").Emitting = true;
    }


    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        _time += dt;
        // grawitacja
        if (_time > GravityDelay)
        {
            Velocity += Vector3.Down * Gravity * dt;
        }

        // ruch
        GlobalPosition += Velocity * dt;

        // obrót strzały w kierunku lotu
        if (Velocity.LengthSquared() > 0.001f)
        {
            LookAt(GlobalPosition + Velocity, Vector3.Up);
        }

        _timeAlive += dt;
        if (_timeAlive >= LifeTime)
            QueueFree();
    }
}
