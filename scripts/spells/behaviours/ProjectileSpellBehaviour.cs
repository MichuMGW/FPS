using Godot;

public class ProjectileSpellBehaviour : ISpellBehaviour
{
    public void PerformCast(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not ProjectileSpellDefinition def)
        {
            GD.PrintErr($"ProjectileSpellBehaviour: SpellDefinition is not ProjectileSpellDefinition ({ctx.Instance.Definition.Id})");
            return;
        }

        if (def.ProjectileScene == null)
        {
            GD.PrintErr($"ProjectileSpellBehaviour: ProjectileScene is null ({def.Id})");
            return;
        }

        Node node = def.ProjectileScene.Instantiate();
        if (node is not Projectile projectile)
        {
            GD.PrintErr($"ProjectileSpellBehaviour: Scene root must be Projectile. Got {node.GetType().Name} ({def.Id})");
            node.QueueFree();
            return;
        }

        ctx.Caster.GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = ctx.Muzzle.GlobalPosition;

        projectile.Configure(def.Element, ctx.Stats.Damage, ctx.Stats.Range);
        var direction = ApplySpread(ctx.Direction, def.ProjectileSpread);

        Vector3 velocity = direction * ctx.Stats.ProjectileSpeed;

        projectile.LookAt(
            projectile.GlobalPosition + direction,
            Vector3.Up,
            true
        );
        projectile.Launch(velocity);
    }

    private Vector3 ApplySpread(Vector3 baseDir, float spreadDegrees)
    {
        var rng = new RandomNumberGenerator();

        if (spreadDegrees <= 0f)
            return baseDir;

        // Losowy yaw/pitch w stopniach
        float yaw   = Mathf.DegToRad(rng.RandfRange(-spreadDegrees, spreadDegrees));
        float pitch = Mathf.DegToRad(rng.RandfRange(-spreadDegrees, spreadDegrees));

        Basis basis = Basis.LookingAt(baseDir, Vector3.Up);

        Vector3 spreadDir = baseDir.Rotated(basis.Y, yaw).Rotated(basis.X, pitch).Normalized();
        return spreadDir;
    }
}
