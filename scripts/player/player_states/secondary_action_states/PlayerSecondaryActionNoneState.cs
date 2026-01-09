using Godot;

public class PlayerSecondaryActionNoneState : IState, IUpdateState
{
    private readonly Player player;

    public PlayerSecondaryActionNoneState(Player playerContext) => player = playerContext;

    public void Enter()
    {
    }
    public void Exit() { }

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
        slot = SpellSlot.RightHand;

        if (player.CurrentMoveStateId == PlayerMoveStateId.Knockback) return false;

        if (Input.IsActionPressed("CastDash"))
        {
            slot = SpellSlot.Dash;
            return true;
        }

        if (Input.IsActionPressed("CastRightSpell"))
        {
            return true;
        }

        return false;
    }
}
