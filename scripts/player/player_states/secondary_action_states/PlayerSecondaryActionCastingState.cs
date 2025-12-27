using Godot;

public class PlayerSecondaryActionCastingState : IState
{
    private readonly Player player;
    private SpellSlot slot;

    public PlayerSecondaryActionCastingState(Player p) => player = p;

    public void Enter()
    {
        slot = player.CurrentSecondaryCastingSlot;

        // Dash: jeśli dash jest osobnym slotem i stanem, to możesz go w ogóle nie wrzucać w tę maszynę
        if (slot == SpellSlot.Dash)
        {
            // albo osobny state dla dash
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (!player.Spells.BeginCast(slot))
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        var def = player.Spells.GetInstance(slot)?.Definition;
        if (def == null) return;

        // animacja startowa tylko zależnie od cast-mode
        switch (def.CastMode)
        {
            case SpellCastMode.Channel:
                player.PlayRightArmAnimation("R_CastChannel");
                break;
            case SpellCastMode.ChargeRelease:
            case SpellCastMode.ChargeAuto:
                player.PlayRightArmAnimation("R_CastPrepare");
                break;
            default:
                player.PlayRightArmAnimation("R_CastProjectile");
                break;
        }
    }

    public void Exit()
    {
        player.Spells.EndCast(slot, CastEndReason.Canceled);
    }

    public void Update(double delta)
    {
        // przykładowy “interrupt”
        if (Input.IsActionJustPressed("CastDash"))
        {
            player.Spells.EndCast(slot, CastEndReason.Interrupted);
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (!IsHeld(slot))
        {
            player.Spells.EndCast(slot, CastEndReason.Released);
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        player.Spells.UpdateCast(slot, (float)delta);
    }

    public void PhysicsUpdate(double delta) { }

    private bool IsHeld(SpellSlot s)
    {
        if (s == SpellSlot.RightHand)
            return Input.IsActionPressed("CastRightSpell");
        return false;
    }
}
