using Godot;

public partial class PathfindComponent : Node
{
    [Export] public NavigationAgent3D Agent;
    [Export] public float TargetReachedThreshold { get; set; } = 0.6f;
    private VelocityComponent _velocity;

    public Node3D Target { get; set; }
    public bool Active { get; set; }
    private CharacterBody3D _body;

    public override void _Ready()
    {
        _body = GetOwner<CharacterBody3D>();
        _velocity = _body.GetNode<VelocityComponent>("VelocityComponent");

        SetPlayerAsTarget();

        Agent.PathDesiredDistance = TargetReachedThreshold;
        Agent.TargetDesiredDistance = TargetReachedThreshold;
    }

    public void Tick(float dt)
    {
        if (Engine.GetFramesDrawn() % 10 != 0) return;

        if (!Active)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
            return;
        }

        if (Target == null)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
            return;
        }

        Agent.TargetPosition = Target.GlobalPosition;

        if (_body.GlobalPosition.DistanceTo(Agent.TargetPosition) <= TargetReachedThreshold)
        {
            _velocity.SetDesiredDirection(Vector3.Zero);
            return;
        }

        Vector3 nextPathPoint = Agent.GetNextPathPosition();
        Vector3 toNext = nextPathPoint - _body.GlobalPosition;
        toNext.Y = 0;

        if (toNext.LengthSquared() < 0.001f)
            _velocity.SetDesiredDirection(Vector3.Zero);
        else
            _velocity.SetDesiredDirection(toNext);
    }

    public void SetPlayerAsTarget()
    {
        Target = GetTree().GetFirstNodeInGroup("player") as Node3D;
    }
}
