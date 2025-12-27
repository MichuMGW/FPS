using Godot;

public class PlayerPrimaryActionCastingState : IState
{
    private readonly Player player;
    private readonly SpellSlot slot = SpellSlot.LeftHand;

    public PlayerPrimaryActionCastingState(Player p) => player = p;

    public void Enter()
    {
        if (!player.Spells.BeginCast(slot))
        {
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
            return;
        }

        // animacja startowa zależna od definicji (tylko kosmetyka)
        var inst = player.Spells.GetInstance(slot);
        var def = inst?.Definition;
        if (def == null) return;

        if (def.CastMode == SpellCastMode.Channel || player.Spells.IsCooldownShort(slot))
            player.PlayLeftArmAnimation("L_CastChannel");
        else
            player.PlayLeftArmAnimation("L_CastProjectile");
    }

    public void Exit()
    {
        player.Spells.EndCast(slot, CastEndReason.Canceled);
    }

    public void Update(double delta)
    {
        if (!Input.IsActionPressed("CastLeftSpell"))
        {
            player.Spells.EndCast(slot, CastEndReason.Released);
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
            return;
        }

        player.Spells.UpdateCast(slot, (float)delta);

        // jeśli chcesz: animacje “tickowane” przy udanym cast, ale to już UI, nie mechanika
    }

    public void PhysicsUpdate(double delta) { }
}
