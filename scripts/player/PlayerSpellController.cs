using System;
using System.Collections.Generic;
using Godot;

public enum SpellSlot
{
    LeftHand,
    RightHand,
    Dash
}

public partial class PlayerSpellController : Node
{
    [Export] public NodePath LeftHandPath;
    [Export] public NodePath RightHandPath;
    [Export] public NodePath CameraPath;
    [Export] public PlayerStatsManager StatsManager;

    private Node3D _leftHand;
    private Node3D _rightHand;
    private Camera3D _camera;

    private readonly Dictionary<SpellSlot, SpellInstance> _equipped = new();

    private readonly Dictionary<SpellBehaviourType, ISpellBehaviour> _behaviours = new();
    private readonly Dictionary<SpellBehaviourType, Func<ISpellBehaviour>> _behaviourFactories = new();

    [Export] public ElementKitDefinition StartingKit;

    public override void _Ready()
    {
        _leftHand = GetNode<Node3D>(LeftHandPath);
        _rightHand = GetNode<Node3D>(RightHandPath);
        _camera = GetNode<Camera3D>(CameraPath);

        RegisterDefaultBehaviours();
        EquipKit(StartingKit);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        foreach (var kv in _equipped)
            kv.Value?.TickCooldown(dt);
    }

    private void RegisterDefaultBehaviours()
    {
        RegisterBehaviourFactory(SpellBehaviourType.Projectile, () => new ProjectileSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Area, () => new AreaSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Beam, () => new BeamSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Dash, () => new DashSpellBehaviour());

        // Explosion jako osobny behaviour:
        // (Jeśli nie masz osobnej wartości w enumie, to dodaj. Inaczej będziesz kombinował jak koń pod górę.)
        RegisterBehaviourFactory(SpellBehaviourType.Explosion, () => new ExplosionSpellBehaviour());
    }

    public void RegisterBehaviourFactory(SpellBehaviourType type, Func<ISpellBehaviour> factory)
    {
        _behaviourFactories[type] = factory;
        _behaviours.Remove(type); // reset cache dla tego typu
    }

    public void EquipKit(ElementKitDefinition kit)
    {
        _equipped[SpellSlot.LeftHand] = kit.LeftHandSpell != null ? new SpellInstance(kit.LeftHandSpell) : null;
        _equipped[SpellSlot.RightHand] = kit.RightHandSpell != null ? new SpellInstance(kit.RightHandSpell) : null;
        _equipped[SpellSlot.Dash] = kit.DashSpell != null ? new SpellInstance(kit.DashSpell) : null;
    }

    public SpellInstance GetInstance(SpellSlot slot)
        => _equipped.TryGetValue(slot, out var inst) ? inst : null;

    public ISpellBehaviour GetBehaviourForDebug(SpellDefinition def) => GetBehaviourFor(def);

    public ISpellBehaviour GetBehaviourFor(SpellDefinition def)
    {
        var type = def.BehaviourType;

        if (_behaviours.TryGetValue(type, out var cached))
            return cached;

        if (!_behaviourFactories.TryGetValue(type, out var factory) || factory == null)
        {
            GD.PrintErr($"No behaviour factory registered for {type} (spell {def.Id}).");
            return null;
        }

        var behaviour = factory.Invoke();
        if (behaviour == null)
        {
            GD.PrintErr($"Behaviour factory for {type} returned null (spell {def.Id}).");
            return null;
        }

        _behaviours[type] = behaviour;
        return behaviour;
    }

    public SpellCastContext BuildContext(SpellSlot slot)
    {
        var instance = GetInstance(slot);
        if (instance == null)
            return default;

        var stats = instance.BuildCastStats(StatsManager);

        Node3D muzzle = slot switch
        {
            SpellSlot.LeftHand => _leftHand,
            SpellSlot.RightHand => _rightHand,
            SpellSlot.Dash => GetOwner<Node3D>(),
            _ => GetOwner<Node3D>()
        };

        Vector3 dir = GetAimDirection(muzzle, stats.Range);

        return new SpellCastContext
        {
            Caster = GetOwner<Node3D>(),
            Muzzle = muzzle,
            Direction = dir,
            Slot = slot,
            Instance = instance,
            Stats = stats,
            Charge01 = 0f
        };
    }

    public bool TryCast(SpellSlot slot, bool applyCooldown = true)
    {
        var inst = GetInstance(slot);
        if (inst == null)
            return false;

        if (!inst.CanCast)
            return false;

        var ctx = BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        var behaviour = GetBehaviourFor(ctx.Instance.Definition);
        if (behaviour == null)
            return false;

        behaviour.PerformCast(ctx);

        if (applyCooldown)
            ApplyCooldown(inst);

        return true;
    }

    public bool Press(SpellSlot slot)
    {
        var ctx = BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        var behaviour = GetBehaviourFor(ctx.Instance.Definition);
        if (behaviour is not IPressSpellBehaviour press)
            return false;

        press.OnPressed(ctx);
        return true;
    }

    public bool Hold(SpellSlot slot, float dt)
    {
        var ctx = BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        var behaviour = GetBehaviourFor(ctx.Instance.Definition);
        if (behaviour is not IHoldSpellBehaviour hold)
            return false;

        hold.OnHeld(ctx, dt);
        return true;
    }

    public bool Release(SpellSlot slot)
    {
        var ctx = BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        var behaviour = GetBehaviourFor(ctx.Instance.Definition);
        if (behaviour is not IReleaseSpellBehaviour release)
            return false;

        release.OnReleased(ctx);
        return true;
    }

    public bool Cancel(SpellSlot slot)
    {
        var ctx = BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        var behaviour = GetBehaviourFor(ctx.Instance.Definition);
        if (behaviour is not ICancelableSpellBehaviour cancel)
            return false;

        cancel.Cancel(ctx);
        return true;
    }

    public void ApplyCooldown(SpellInstance instance)
    {
        float cdr = StatsManager.GetStat(StatId.CooldownReduction);
        float cdrMult = 1f - cdr;
        instance.PutOnCooldown(cdrMult);
    }

    private Vector3 GetAimDirection(Node3D muzzle, float range)
    {
        var raycast = GetTree().GetFirstNodeInGroup("player_ray") as RayCast3D;
        if (raycast != null && raycast.IsColliding())
        {
            var hit = raycast.GetCollisionPoint();
            return (hit - muzzle.GlobalPosition).Normalized();
        }

        var targetPos = _camera.GlobalTransform.Origin + (-_camera.GlobalTransform.Basis.Z * range);
        return (targetPos - muzzle.GlobalPosition).Normalized();
    }

    public bool IsCooldownShort(SpellSlot slot)
    {
        var instance = GetInstance(slot);
        return instance.Definition.BaseCooldown < 0.4f;
    }
}
