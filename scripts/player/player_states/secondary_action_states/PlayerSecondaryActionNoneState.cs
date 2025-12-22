using Godot;

public class PlayerSecondaryActionNoneState : IState
{
    private readonly Player player;

    public PlayerSecondaryActionNoneState(Player playerContext) => player = playerContext;

    public void Enter()
    {
        player.PlayRightArmAnimation("R_Idle");
    }
    public void Exit() { }
    public void PhysicsUpdate(double delta) { }

    public void Update(double delta)
    {
        if (TryGetSecondaryPressedSlot(out var slot))
        {
            player.CurrentSecondaryCastingSlot = slot;
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.Casting);
        }
    }

    private bool TryGetSecondaryPressedSlot(out SpellSlot slot)
    {
        // priorytet: dash -> prawy
        if (Input.IsActionJustPressed("CastDash"))
        {
            slot = SpellSlot.Dash;
            return true;
        }

        if (Input.IsActionPressed("CastRightSpell"))
        {
            slot = SpellSlot.RightHand;
            return true;
        }

        slot = SpellSlot.RightHand;
        return false;
    }
}
