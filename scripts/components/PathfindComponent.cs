using Godot;
using System;

public partial class PathfindComponent : Node
{
    [Export] public NavigationAgent3D Agent;
    [Export] public float TargetReachedThreshold { get; set; } = 0.6f;
    [Export] private VelocityComponent _velocity;

    public Node3D Target { get; set; }
    private CharacterBody3D _body;

    public override void _Ready()
    {
        _body = GetOwner<CharacterBody3D>();

        Target = GetTree().GetFirstNodeInGroup("player") as Node3D;

        Agent.PathDesiredDistance = TargetReachedThreshold;
        Agent.TargetDesiredDistance = TargetReachedThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        Agent.TargetPosition = Target.GlobalPosition;

        // jeżeli jesteśmy już blisko celu – hamujemy
        if (_body.GlobalPosition.DistanceTo(Agent.TargetPosition) <= TargetReachedThreshold)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
            return;
        }

        // następny punkt ścieżki
        Vector3 nextPathPoint = Agent.GetNextPathPosition();
        Vector3 toNext = nextPathPoint - _body.GlobalPosition;
        toNext.Y = 0;

        if (toNext.LengthSquared() < 0.001f)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
        }
        else
        {
            _velocity.SetDesiredDirection(toNext);
        }
    }


}
