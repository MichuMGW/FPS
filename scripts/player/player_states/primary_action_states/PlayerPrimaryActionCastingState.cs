using Godot;

public class PlayerPrimaryActionCastingState : IState
{
    private readonly Player player;

    public PlayerPrimaryActionCastingState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter() { }
    public void Exit() { }
    public void PhysicsUpdate(double delta) { }

    public void Update(double delta)
    {
        if (!Input.IsActionPressed("CastLeftSpell"))
        {
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
            return;
        }

        player.Spells.TryCast(SpellSlot.LeftHand);
    }
}
