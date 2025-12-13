using Godot;

public class PlayerSecondaryActionNoneState : IState
{
    private readonly Player player;

    public PlayerSecondaryActionNoneState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter() { }
    public void Exit() { }
    public void PhysicsUpdate(double delta) { }

    public void Update(double delta)
    {
        SpellSlot slot;
        if (!TryGetSecondaryPressedSlot(out slot))
            return;

        player.CurrentSecondaryCastingSlot = slot;
        player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.Casting);
    }

    private bool TryGetSecondaryPressedSlot(out SpellSlot slot)
    {
        // wg priorytetów
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

        if (Input.IsActionJustPressed("CastShield"))
        {
            slot = SpellSlot.Shield;
            return true;
        }

        if (Input.IsActionJustPressed("CastBuff"))
        {
            slot = SpellSlot.Buff;
            return true;
        }

        slot = SpellSlot.RightHand;
        return false;
    }
}
