using Godot;

public class PlayerPrimaryActionNoneState : IState
{
    private readonly Player player;

    public PlayerPrimaryActionNoneState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        player.PlayLeftArmAnimation("L_Idle");
    }
    public void Exit() { }
    public void PhysicsUpdate(double delta) { }

    public void Update(double delta)
    {
        if (Input.IsActionPressed("CastLeftSpell"))
        {
            player.CurrentPrimaryCastingSlot = SpellSlot.LeftHand;
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.Casting);
        }
    }
}
