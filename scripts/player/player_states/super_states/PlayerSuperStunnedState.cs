using Godot;

public class PlayerSuperStunnedState : IState
{
    private readonly Player player;

    public PlayerSuperStunnedState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        // Blokuj akcje natychmiast
        player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
        player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);

        // Możesz też wymusić konkretny move state (np. Airborne jeśli gracz jest w powietrzu)
        // Na start: zostaw co było, ale zablokuj input w movement state (patrz niżej)

        // Opcjonalnie: animacja “stun”
        // player.PlayStunAnimation();
    }

    public void Exit()
    {
        // Opcjonalnie: wróć do normalnych animacji
    }

    public void Update(double delta)
    {
        float dt = (float)delta;

        if (player.StunTimeLeft > 0f)
            player.StunTimeLeft = Mathf.Max(0f, player.StunTimeLeft - dt);

        if (player.StunTimeLeft <= 0f)
            player.ChangeSuperState(PlayerSuperStateId.Alive);
    }

    public void PhysicsUpdate(double delta)
    {
        // W stun: nadal działa knockback damping (masz to w Player._PhysicsProcess)
        // A ruch z inputu ma być wyłączony (zrobimy to w MoveState)
    }
}
