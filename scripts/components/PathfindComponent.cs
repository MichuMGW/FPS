using Godot;
using System;

public partial class PathfindComponent : Node
{
    [Export] public NavigationAgent3D Agent;
    [Export] public float TargetReachedThreshold { get; set; } = 0.6f;
    [Export] private VelocityComponent _velocity;

    public Node3D Target { get; set; }
    public bool Active { get; set; }
    private CharacterBody3D _body; //Można tutaj dać [Export] i podpiąć w edytorze

    public override void _Ready()
    {
        _body = GetOwner<CharacterBody3D>();

        SetPlayerAsTarget();

        Agent.PathDesiredDistance = TargetReachedThreshold;
        Agent.TargetDesiredDistance = TargetReachedThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Active)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
            return;
        }

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

    public void SetPlayerAsTarget()
    {
        Target = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }


}
