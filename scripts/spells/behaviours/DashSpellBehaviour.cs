using Godot;

public class DashSpellBehaviour : ISpellBehaviour
{
    public void PerformCast(SpellCastContext ctx)
    {
        if (ctx.Caster is Player player && ctx.Instance.Definition is DashSpellDefinition dashDef)
        {
            Vector3 dir = ctx.Direction;
            dir.Y = 0f;
            if (dir.LengthSquared() < 0.0001f) dir = -player.GlobalTransform.Basis.Z;
            player.StartDash(dashDef, dir);
        }
    }
}

