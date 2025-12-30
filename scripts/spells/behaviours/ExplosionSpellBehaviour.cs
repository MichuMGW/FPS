using Godot;
using System.Collections.Generic;

public class ExplosionSpellBehaviour : ISpellBehaviour, IPressSpellBehaviour, IHoldSpellBehaviour, IReleaseSpellBehaviour, ICancelableSpellBehaviour
{
    private readonly struct Key
    {
        public readonly ulong CasterId;
        public readonly SpellSlot Slot;
        public Key(ulong casterId, SpellSlot slot) { CasterId = casterId; Slot = slot; }
        public override int GetHashCode() => (CasterId.GetHashCode() * 397) ^ Slot.GetHashCode();
        public override bool Equals(object obj) => obj is Key o && o.CasterId == CasterId && o.Slot == Slot;
    }

    private class CastState
    {
        public Node3D Indicator;
        public float Radius;
        public bool HasValidPoint;
        public Vector3 Point;
    }

    private readonly Dictionary<Key, CastState> _states = new();

    public void PerformCast(SpellCastContext ctx)
    {
        // Explosion jest ChargeRelease, więc cast "instant" przez PerformCast jest błędem.
        // Jeśli ktoś ustawi CastMode inaczej, to ma dostać błąd, a nie magiczny behaviour.
        GD.PrintErr($"ExplosionSpellBehaviour.PerformCast called for {ctx.Instance.Definition.Id}. Explosion should be ChargeRelease and use Press/Hold/Release.");
    }

    public void OnPressed(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not ExplosionSpellDefinition def)
        {
            GD.PrintErr($"ExplosionSpellBehaviour: SpellDefinition is not ExplosionSpellDefinition ({ctx.Instance.Definition.Id})");
            return;
        }

        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);

        // Jeśli już celujesz, nie twórz kolejnego stanu
        if (_states.ContainsKey(key))
            return;

        CastState state = new CastState { Radius = def.StartRadius };

        if (def.IndicatorScene != null)
        {
            Node node = def.IndicatorScene.Instantiate();
            if (node is Node3D indicator)
            {
                indicator.TopLevel = true; // odetnij potencjalne skale z parentów
                ctx.Caster.GetTree().CurrentScene.AddChild(indicator);
                state.Indicator = indicator;
            }
            else
            {
                GD.PrintErr($"ExplosionSpellBehaviour: IndicatorScene root must be Node3D. Got {node.GetType().Name} ({def.Id})");
                node.QueueFree();
            }
        }

        _states[key] = state;
        UpdateIndicator(ctx, def, state);
    }

    public void OnHeld(SpellCastContext ctx, float dt)
    {
        if (ctx.Instance.Definition is not ExplosionSpellDefinition def)
            return;

        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (!_states.TryGetValue(key, out var state))
            return;

        state.Radius = Mathf.Min(state.Radius + def.GrowthRate * dt, def.MaxRadius);
        UpdateIndicator(ctx, def, state);
    }

    public bool OnReleased(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not ExplosionSpellDefinition def)
            return false;

        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (!_states.TryGetValue(key, out var state))
            return false;

        // najpierw sprzątnij indicator
        Cleanup(key);

        // jak nie ma valid point, to NIE castujemy
        if (!state.HasValidPoint)
            return false;

        if (def.ExplosionScene == null)
        {
            GD.PrintErr($"ExplosionSpellBehaviour: ExplosionScene is null ({def.Id})");
            return false;
        }

        Node node = def.ExplosionScene.Instantiate();
        if (node is not ExplosionAoE aoe)
        {
            GD.PrintErr($"ExplosionSpellBehaviour: ExplosionScene root must be ExplosionAoE. Got {node.GetType().Name} ({def.Id})");
            node.QueueFree();
            return false;
        }

        aoe.TopLevel = true;
        ctx.Caster.GetTree().CurrentScene.AddChild(aoe);

        aoe.GlobalPosition = state.Point;
        aoe.Configure(
            def.Element,
            ctx.Stats.Damage,
            state.Radius,
            ctx.Stats.CritChance,
            ctx.Stats.CritMultiplier,
            def.StatusProfile,
            def.BurningDotMultiplier,
            def.BleedDotMultiplier,
            def.SlowMultiplierBonus,
            def.EarthBuildupPerHit
        );

        return true;
    }

    public void Cancel(SpellCastContext ctx)
    {
        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        Cleanup(key);
    }

    private void Cleanup(Key key)
    {
        if (!_states.TryGetValue(key, out var state))
            return;

        _states.Remove(key);

        if (state.Indicator != null && GodotObject.IsInstanceValid(state.Indicator))
            state.Indicator.QueueFree();
    }

    private void UpdateIndicator(SpellCastContext ctx, ExplosionSpellDefinition def, CastState state)
    {
        bool valid = TryGetPlacementPoint(ctx, def, out Vector3 hitPoint);

        state.HasValidPoint = valid;
        state.Point = hitPoint;

        if (state.Indicator == null)
            return;

        state.Indicator.GlobalPosition = hitPoint;
        UpdateIndicatorVisual(state.Indicator, state.Radius, valid);
    }

    private bool TryGetPlacementPoint(SpellCastContext ctx, ExplosionSpellDefinition def, out Vector3 point)
    {
        point = ctx.Muzzle.GlobalPosition + ctx.Direction.Normalized() * def.MaxPlacementDistance;

        World3D world = ctx.Caster.GetWorld3D();
        if (world == null)
            return false;

        var space = world.DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(
            ctx.Muzzle.GlobalPosition,
            ctx.Muzzle.GlobalPosition + ctx.Direction.Normalized() * def.MaxPlacementDistance
        );

        query.CollisionMask = def.PlacementMask;
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;

        var result = space.IntersectRay(query);
        if (result.Count == 0)
            return false;

        point = (Vector3)result["position"];
        return true;
    }

    private void UpdateIndicatorVisual(Node3D indicator, float radius, bool valid)
    {
        indicator.Scale = new Vector3(radius, radius, radius);

        MeshInstance3D mesh = indicator.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        if (mesh == null)
        {
            GD.PrintErr("ExplosionSpellBehaviour: Indicator is missing MeshInstance3D child.");
            return;
        }

        StandardMaterial3D mat = mesh.GetActiveMaterial(0) as StandardMaterial3D;
        if (mat == null)
        {
            mat = new StandardMaterial3D
            {
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha
            };
            mesh.SetSurfaceOverrideMaterial(0, mat);
        }

        mat.AlbedoColor = valid ? new Color(0, 1, 0, 0.25f) : new Color(1, 0, 0, 0.25f);
    }
}
