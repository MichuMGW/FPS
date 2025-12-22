using Godot;

public class DashSpellBehaviour : ISpellBehaviour
{
    public void PerformCast(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not DashSpellDefinition def)
        {
            GD.PrintErr($"DashSpellBehaviour: SpellDefinition is not DashSpellDefinition ({ctx.Instance.Definition.Id})");
            return;
        }

        Player player = ctx.Caster as Player;
        if (player == null)
            return;

        float dashSpeed = Mathf.Max(1f, def.DashSpeed);
        Vector3 dir = ctx.Direction.Normalized();

        Vector3 current = player.Velocity;
        float y = def.KeepYVelocity ? current.Y : 0f;

        player.Velocity = new Vector3(dir.X * dashSpeed, y, dir.Z * dashSpeed);

        if (player.Knockback != null)
            player.Knockback.ClearKnockback();
    }
}
