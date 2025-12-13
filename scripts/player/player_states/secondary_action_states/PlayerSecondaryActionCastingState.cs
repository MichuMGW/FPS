using Godot;

public class PlayerSecondaryActionCastingState : IState
{
    private readonly Player player;
    private bool castedOnce;

    public PlayerSecondaryActionCastingState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        castedOnce = false;
    }

    public void Exit() { }
    public void PhysicsUpdate(double delta) { }

    public void Update(double delta)
    {
        SpellSlot slot = player.CurrentSecondaryCastingSlot;

        if (slot == SpellSlot.Buff || slot == SpellSlot.Dash)
        {
            if (!castedOnce)
            {
                player.Spells.TryCast(slot);
                castedOnce = true;
            }

            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        // RightHand / Shield jako hold/spam
        if (!IsSecondaryHeld(slot))
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        player.Spells.TryCast(slot);
    }

    private bool IsSecondaryHeld(SpellSlot slot)
    {
        if (slot == SpellSlot.RightHand)
            return Input.IsActionPressed("CastRightSpell");

        return false;
    }
}
