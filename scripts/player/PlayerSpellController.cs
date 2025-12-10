using System.Collections.Generic;
using Godot;

public enum SpellSlot
{
    LeftHand,
    RightHand,
    Shield,
    Buff
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
    private readonly Dictionary<SpellBehaviourType, ISpellBehaviour> _behaviourCache = new();

    public override void _Ready()
    {
        _leftHand = GetNode<Node3D>(LeftHandPath);
        _rightHand = GetNode<Node3D>(RightHandPath);
        _camera = GetNode<Camera3D>(CameraPath);
    }

    public override void _Process(double delta)
    {
        foreach (var kv in _equipped)
        {
            kv.Value?.TickCooldown((float)delta);
        }
    }

    public void EquipKit(ElementKitDefinition kit)
    {
        _equipped[SpellSlot.LeftHand] = kit.LeftHandSpell != null ? new SpellInstance(kit.LeftHandSpell)  : null;
        _equipped[SpellSlot.RightHand] = kit.RightHandSpell != null ? new SpellInstance(kit.RightHandSpell) : null;
        _equipped[SpellSlot.Shield] = kit.ShieldSpell != null ? new SpellInstance(kit.ShieldSpell) : null;
        _equipped[SpellSlot.Buff] = kit.BuffSpell != null ? new SpellInstance(kit.BuffSpell) : null;
    }

    public bool TryCast(SpellSlot slot)
    {
        if (!_equipped.TryGetValue(slot, out var spell) || spell == null)
            return false;

        if (!spell.CanCast)
            return false;

        var def = spell.Definition;
        var behaviour = GetBehaviourFor(def);
        if (behaviour == null)
            return false;

        var stats = spell.BuildCastStats(StatsManager);

        Node3D muzzle = slot switch
        {
            SpellSlot.LeftHand  => _leftHand,
            SpellSlot.RightHand => _rightHand,
            SpellSlot.Shield    => GetOwner<Node3D>(),
            SpellSlot.Buff      => GetOwner<Node3D>(),
            _ => GetOwner<Node3D>()
        };

        Vector3 dir = GetAimDirection(muzzle, stats.Range);

        var ctx = new SpellCastContext
        {
            Caster = GetOwner<Node3D>(),
            Muzzle = muzzle,
            Direction = dir,
            Instance = spell,
            Stats = stats
        };

        behaviour.PerformCast(ctx);

        float cdr = StatsManager.GetStat(StatId.CooldownReduction);
        float cdrMult = 1f - cdr;
        spell.PutOnCooldown(cdrMult);

        return true;
    }

    private ISpellBehaviour GetBehaviourFor(SpellDefinition def)
    {
        var type = def.BehaviourType;

        if (_behaviourCache.TryGetValue(type, out var behaviour))
            return behaviour;

        behaviour = type switch
        {
            SpellBehaviourType.Projectile => new ProjectileSpellBehaviour(),
            // SpellBehaviourType.Beam => new BeamSpellBehaviour(),
            // SpellBehaviourType.Area => new AreaSpellBehaviour(),
            // SpellBehaviourType.Dash => new DashSpellBehaviour(),
            // SpellBehaviourType.Shield => new ShieldSpellBehaviour(),
            // SpellBehaviourType.Buff => new BuffSpellBehaviour(),
            _ => null
        };

        if (behaviour == null)
        {
            GD.PrintErr($"No behaviour mapped for SpellBehaviourType {type} (spell {def.Id})");
            return null;
        }

        _behaviourCache[type] = behaviour;
        return behaviour;
    }

    private Vector3 GetAimDirection(Node3D muzzle, float range)
    {
        var raycast = GetTree().GetFirstNodeInGroup("player_ray") as RayCast3D;
        if (raycast != null && raycast.IsColliding())
        {
            var hit = raycast.GetCollisionPoint();
            return (hit - muzzle.GlobalPosition).Normalized();
        }

        var targetPos = _camera.GlobalTransform.Origin 
                        + (-_camera.GlobalTransform.Basis.Z * range);

        return (targetPos - muzzle.GlobalPosition).Normalized();
    }
}


// TO WRZUCIMY DO KLASY GRACZA
// if (Input.IsActionJustPressed("CastLeftSpell"))
//     _spellController.TryCast(SpellSlot.LeftHand);

// if (Input.IsActionJustPressed("CastRightSpell"))
//     _spellController.TryCast(SpellSlot.RightHand);

// if (Input.IsActionJustPressed("CastShield"))
//     _spellController.TryCast(SpellSlot.Shield);

// if (Input.IsActionJustPressed("CastBuff"))
//     _spellController.TryCast(SpellSlot.Buff);
