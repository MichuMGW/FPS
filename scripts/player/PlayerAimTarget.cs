using Godot;
using System;

public partial class PlayerAimTarget : Node3D
{
    private Node3D _owner;

    [Export] private float _easingStrength = 5.0f;
    [Export] private float _offset = 0.5f;

    private Vector3 _velocity;

    public override void _Ready()
    {
        _owner = GetParent() as Node3D;

        TopLevel = true;
        GlobalPosition = _owner.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        float t = 1f - Mathf.Exp(-_easingStrength * (float)delta);
        GlobalPosition = GlobalPosition.Lerp(_owner.GlobalPosition + new Vector3(0, _offset, 0), t);
    }
}
