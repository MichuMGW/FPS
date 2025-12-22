using Godot;
using System.Collections.Generic;

public class AreaSpellBehaviour : ISpellBehaviour, IHoldSpellBehaviour, ICancelableSpellBehaviour
{
    private readonly struct Key
    {
        public readonly ulong CasterId;
        public readonly SpellSlot Slot;
        public Key(ulong casterId, SpellSlot slot) { CasterId = casterId; Slot = slot; }
        public override int GetHashCode() => (CasterId.GetHashCode() * 397) ^ Slot.GetHashCode();
        public override bool Equals(object obj) => obj is Key o && o.CasterId == CasterId && o.Slot == Slot;
    }

    private readonly Dictionary<Key, AreaEmitter> _active = new();

    public void PerformCast(SpellCastContext ctx)
    {
        if (ctx.Instance.Definition is not AreaSpellDefinition def)
        {
            GD.PrintErr($"AreaSpellBehaviour: SpellDefinition is not AreaSpellDefinition ({ctx.Instance.Definition.Id})");
            return;
        }

        if (def.EmitterScene == null)
        {
            GD.PrintErr($"AreaSpellBehaviour: EmitterScene is null ({def.Id})");
            return;
        }

        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (_active.TryGetValue(key, out var existing) && GodotObject.IsInstanceValid(existing))
            return;

        Node node = def.EmitterScene.Instantiate();
        if (node is not AreaEmitter emitter)
        {
            GD.PrintErr($"AreaSpellBehaviour: Scene root must be AreaEmitter. Got {node.GetType().Name} ({def.Id})");
            node.QueueFree();
            return;
        }

        ctx.Caster.GetTree().CurrentScene.AddChild(emitter);

        float tickRate = Mathf.Max(0.01f, def.TickRate);
        emitter.Configure(def.Element, ctx.Stats.Damage, tickRate);
        // jeśli masz w AreaEmitter promień:
        // emitter.SetRadius(def.AuraRadius);

        emitter.FollowMuzzle(ctx.Muzzle, ctx.Direction);
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

        emitter.FollowMuzzle(ctx.Muzzle, ctx.Direction);
    }

    public void Cancel(SpellCastContext ctx)
    {
        Key key = new Key(ctx.Caster.GetInstanceId(), ctx.Slot);
        if (!_active.TryGetValue(key, out var emitter))
            return;

        _active.Remove(key);

        if (GodotObject.IsInstanceValid(emitter))
            emitter.QueueFree();
    }
}
