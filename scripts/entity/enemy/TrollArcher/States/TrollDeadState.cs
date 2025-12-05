using Godot;

public partial class TrollDeadState : IState
{
    private TrollArcher _owner;
    public TrollDeadState(TrollArcher owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.DisableSpineLookAtTarget();
        _owner.Hurtbox.Active = false;
        RotateTowardPlayer();

        _owner.Bow.Visible = false;

        var bowInstance = _owner.BowScene.Instantiate() as Node3D;
        _owner.AddChild(bowInstance);
        bowInstance.GlobalPosition = _owner.GlobalPosition;

        var bowAnimation = bowInstance.GetNode<AnimationPlayer>("bow/AnimationPlayer");

        bowAnimation.Play("Troll_Die");
        _owner.TrollAnimation.Play("Troll_Die", 0.3f);
    }

    public void Exit()
    {
        _owner.QueueFree();
    }

    private void RotateTowardPlayer()
    {
        var targetPos = _owner.Player.GlobalPosition;
        targetPos.Y = _owner.GlobalPosition.Y;

        _owner.LookAt(targetPos, Vector3.Up, true);
    }

    public void PhysicsUpdate(double delta){}

    public void Update(double delta){}
}