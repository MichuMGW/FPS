using Godot;
using System.Collections.Generic;

public class BeamSpellBehaviour : ISpellBehaviour, IHoldSpellBehaviour, ICancelableSpellBehaviour
{
    private readonly struct Key
    {
        public readonly ulong CasterId;
        public readonly SpellSlot Slot;
        public Key(ulong casterId, SpellSlot slot) { CasterId = casterId; Slot = slot; }
        public override int GetHashCode() => (CasterId.GetHashCode() * 397) ^ Slot.GetHashCode();
        public override bool Equals(object obj) => obj is Key o && o.CasterId == CasterId && o.Slot == Slot;
    }

    private readonly Dictionary<Key, BeamEmitter> _active = new();

    public void PerformCast(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not BeamSpellDefinition def)
        {
            GD.PrintErr($"BeamSpellBehaviour: SpellDefinition is not BeamSpellDefinition ({ctx.Instance.Definition.Id})");
            return;
        }

        if (def.EmitterScene == null)
        {
            GD.PrintErr($"BeamSpellBehaviour: EmitterScene is null ({def.Id})");
            return;
        }

        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (_active.TryGetValue(key, out var existing) && GodotObject.IsInstanceValid(existing))
            return;

        Node node = def.EmitterScene.Instantiate();
        if (node is not BeamEmitter emitter)
        {
            GD.PrintErr($"BeamSpellBehaviour: Scene root must be BeamEmitter. Got {node.GetType().Name} ({def.Id})");
            node.QueueFree();
            return;
        }

        ctx.Caster.GetTree().CurrentScene.AddChild(emitter);

        float tickRate = Mathf.Max(0.01f, def.TickRate);
        float maxLen = Mathf.Max(1f, ctx.Stats.Range);

        emitter.Configure(
            def.Element,
            ctx.Stats.Damage,
            tickRate,
            maxLen,
            def.BeamWidth,
            def.PierceCount
        );

        emitter.Start();

        emitter.UpdateBeam(ctx.Muzzle, ctx.Direction);
        _active[key] = emitter;
    }


    public void OnHeld(SpellCastContext ctx, float dt)
    {
        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (!_active.TryGetValue(key, out var emitter))
            return;

        if (!GodotObject.IsInstanceValid(emitter))
        {
            _active.Remove(key);
            return;
        }

        emitter.UpdateBeam(ctx.Muzzle, ctx.Direction);
    }

    public void Cancel(SpellCastContext ctx)
    {
        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (!_active.TryGetValue(key, out var emitter))
            return;

        _active.Remove(key);

        emitter.Stop();
        
        if (GodotObject.IsInstanceValid(emitter))
            emitter.QueueFree();
    }
}
