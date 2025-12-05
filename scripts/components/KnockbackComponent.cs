using Godot;
using System;

public partial class KnockbackComponent : Node
{
    [Export] public CharacterBody3D Body { get; set; }
    [Export] public float Damping { get; set; } = 8f;

    private Vector3 _knockbackVelocity = Vector3.Zero;

    public void ApplyKnockback(Vector3 impulse)
    {
        _knockbackVelocity = impulse;
    }

    public void PhysicsUpdate(double delta)
    {
        if (Body == null)
            return;

        Body.Velocity += _knockbackVelocity;

        _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, (float)(Damping * delta));
    }
}
