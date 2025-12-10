using Godot;
using System;

public class ProjectileSpellBehaviour : ISpellBehaviour
{
    public void PerformCast(SpellCastContext ctx)
    {
        var def = ctx.Instance.Definition;
        var projectileScene = def.ProjectileScene;
        if (projectileScene == null)
        {
            GD.PrintErr($"Spell {def.DisplayName} has no projectile scene");
            return;
        }

        var projectile = projectileScene.Instantiate<Projectile>();
        ctx.Caster.GetTree().CurrentScene.AddChild(projectile);

        projectile.GlobalTransform = ctx.Muzzle.GlobalTransform;
        projectile.Element = def.Element;
        projectile.Damage = ctx.Stats.Damage;
        projectile.Range = ctx.Stats.Range;

        // spread możesz liczyć tu albo wcześniej
        Vector3 dir = ctx.Direction.Normalized();
        projectile.Initialize(dir * ctx.Stats.ProjectileSpeed);
    }
}
