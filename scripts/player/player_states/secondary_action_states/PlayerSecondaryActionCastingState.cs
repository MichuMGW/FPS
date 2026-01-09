using Godot;

public class PlayerSecondaryActionCastingState : IState, IUpdateState
{
    private readonly Player player;
    private SpellSlot slot;

    private bool _ended;
    private CastEndReason _endReason;

    public PlayerSecondaryActionCastingState(Player p) => player = p;

    public void Enter()
    {
        _ended = false;
        _endReason = CastEndReason.Canceled;

        slot = player.CurrentSecondaryCastingSlot;

        // Dash: jeśli dash jest osobnym slotem i stanem, to możesz go w ogóle nie wrzucać w tę maszynę
        if (!player.Spells.BeginCast(slot))
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (slot == SpellSlot.Dash)
        {
            _ended = false;
            player.Spells.BeginCast(slot);
            player.PlayRightArmAnimation("R_Idle");
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }


        var def = player.Spells.GetInstance(slot)?.Definition;
        if (def == null) return;

        // animacja startowa tylko zależnie od cast-mode
        switch (def.CastMode)
        {
            case SpellCastMode.Instant:
                player.PlayRightArmAnimation("R_CastProjectile_Instant");
                break;
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
        if (!_ended)
        {
            player.Spells.EndCast(slot, CastEndReason.Canceled);
        }
    }

    public void Update(double delta)
    {
        // przykładowy “interrupt”
        if (Input.IsActionJustPressed("CastDash"))
        {
            _ended = true;
            _endReason = CastEndReason.Interrupted;
            player.Spells.EndCast(slot, _endReason);
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (!IsHeld(slot))
        {
            _ended = true;
            _endReason = CastEndReason.Released;
            player.Spells.EndCast(slot, _endReason);

            var castMode = player.Spells.GetInstance(slot)?.Definition.CastMode;
            switch (castMode)
            {
                
                case SpellCastMode.ChargeRelease:
                case SpellCastMode.ChargeAuto:
                    player.PlayRightArmAnimation("R_CastRelease");
                    break;
                default:
                    player.PlayRightArmAnimation("R_Idle");
                    break;
            }

            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        player.Spells.UpdateCast(slot, (float)delta);
    }

    private bool IsHeld(SpellSlot s)
    {
        if (s == SpellSlot.RightHand)
            return Input.IsActionPressed("CastRightSpell");
        return false;
    }
}
