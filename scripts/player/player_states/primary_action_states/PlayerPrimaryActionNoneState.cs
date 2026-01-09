using Godot;

public class PlayerPrimaryActionNoneState : IState, IUpdateState
{
    private readonly Player player;

    public PlayerPrimaryActionNoneState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        
    }
    public void Exit() { }

    public void Update(double delta)
    {
        if (Input.IsActionPressed("CastLeftSpell") && player.CurrentMoveStateId != PlayerMoveStateId.Knockback)
        {
            player.CurrentPrimaryCastingSlot = SpellSlot.LeftHand;
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.Casting);
        }
    }
}
