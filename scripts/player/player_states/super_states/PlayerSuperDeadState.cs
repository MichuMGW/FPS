using Godot;

public class PlayerSuperDeadState : IState
{
    private readonly Player player;

    public PlayerSuperDeadState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        player.ChangeMoveState(PlayerMoveStateId.Grounded);
        player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
        player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);

        if (player.Health != null && player.Health.Hurtbox != null)
            player.Health.Hurtbox.Active = false;

        player.CollisionMask = PhysicsLayers.TERRAIN;

    }

    public void Exit()
    {
        if (player.Health != null && player.Health.Hurtbox != null)
            player.Health.Hurtbox.Active = true;
    }

    public void Update(double delta)
    {
    }

    public void PhysicsUpdate(double delta)
    {
    }
}
