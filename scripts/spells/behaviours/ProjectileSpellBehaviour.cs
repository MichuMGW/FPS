using Godot;

public class ProjectileSpellBehaviour : ISpellBehaviour
{
    private readonly RandomNumberGenerator rng = new RandomNumberGenerator();
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

        var dir = ApplySpread(ctx.Direction, def.ProjectileSpread);

        projectile.LookAt(projectile.GlobalPosition + dir, Vector3.Up, true);

        projectile.Configure(
            element: def.Element,
            damage: ctx.Stats.Damage,
            range: ctx.Stats.Range,
            pierce: Mathf.RoundToInt(ctx.Stats.PierceCount),

            enableRehit: ctx.Stats.EnableRehit,
            rehitIntervalSeconds: Mathf.Max(0.01f, ctx.Stats.RehitIntervalSeconds),

            scaleOverTime: ctx.Stats.ScaleOverTime,
            startScale: ctx.Stats.StartScale,
            endScale: ctx.Stats.EndScale,
            scaleDurationSeconds: ctx.Stats.ScaleDurationSeconds,

            explosionScene: def.ExplosionScene, // scena z definicji
            explosionDamageMultiplier: ctx.Stats.ExplosionDamageMultiplier,
            explodeOnEnemy: ctx.Stats.ExplodeOnEnemyHit,
            explodeOnWorld: ctx.Stats.ExplodeOnWorldHit,

            dieOnWorldHit: ctx.Stats.DieOnWorldHit,
            explosionLifetimeSeconds: ctx.Stats.ExplosionLifetimeSeconds
        );

        var hitbox = projectile.GetNodeOrNull<HitboxComponent>("HitboxComponent");
        if (hitbox != null)
        {
            hitbox.CritChance = ctx.Stats.CritChance;
            hitbox.CritMultiplier = ctx.Stats.CritMultiplier;
            hitbox.StatusProfile = def.StatusProfile;          // dopnij do SpellDefinition
            hitbox.BurningDotMultiplier = def.BurningDotMultiplier;    // dopnij do SpellDefinition
            hitbox.BleedDotMultiplier = def.BleedDotMultiplier;
            hitbox.SlowBonus = def.SlowMultiplierBonus;                // dopnij do SpellDefinition
            hitbox.EarthBuildupPerHit = def.EarthBuildupPerHit;        // dopnij do SpellDefinition
        }


        Vector3 velocity = dir * ctx.Stats.ProjectileSpeed;
        projectile.Launch(velocity);
    }

    private Vector3 ApplySpread(Vector3 baseDir, float spreadDegrees)
    {
        if (spreadDegrees <= 0f)
            return baseDir;

        float yaw = Mathf.DegToRad(rng.RandfRange(-spreadDegrees, spreadDegrees));
        float pitch = Mathf.DegToRad(rng.RandfRange(-spreadDegrees, spreadDegrees));

        Basis basis = Basis.LookingAt(baseDir, Vector3.Up);
        return baseDir.Rotated(basis.Y, yaw).Rotated(basis.X, pitch).Normalized();
    }
}
