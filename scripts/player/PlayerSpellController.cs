using System;
using System.Collections.Generic;
using Godot;

public enum SpellSlot { LeftHand, RightHand, Dash }

public enum CastEndReason { Released, Canceled, Interrupted, Casted }

public partial class PlayerSpellController : Node
{
    [Signal] public delegate void SpellRecastedEventHandler(int slot);

    [Export] public NodePath LeftHandPath;
    [Export] public NodePath RightHandPath;
    [Export] public NodePath CameraPath;
    [Export] public PlayerStatsManager StatsManager;
    [Export] public ElementKitDefinition StartingKit;

    private Node3D _leftHand;
    private Node3D _rightHand;
    private Camera3D _camera;
    private RayCast3D _raycast;
    private Node3D _caster;

    private readonly Dictionary<SpellSlot, SpellInstance> _equipped = new();

    private readonly Dictionary<SpellBehaviourType, ISpellBehaviour> _behaviours = new();
    private readonly Dictionary<SpellBehaviourType, Func<ISpellBehaviour>> _behaviourFactories = new();

    // aktywna sesja castu per slot
    private readonly Dictionary<SpellSlot, SpellCastSession> _activeSessions = new();

    // cooldown ticking optymalizacja
    private int _cooldownsActiveCount = 0;

    public override void _Ready()
    {
        _leftHand = GetNode<Node3D>(LeftHandPath);
        _rightHand = GetNode<Node3D>(RightHandPath);
        _camera = GetNode<Camera3D>(CameraPath);

        _caster = GetOwner<Node3D>();
        _raycast = GetTree().GetFirstNodeInGroup("player_ray") as RayCast3D;

        RegisterDefaultBehaviours();
        EquipKit(StartingKit);

        SetProcess(false); // start: nie ma po co tykać
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (_cooldownsActiveCount <= 0)
        {
            SetProcess(false);
            return;
        }

        _cooldownsActiveCount = 0;

        foreach (var kv in _equipped)
        {
            var inst = kv.Value;
            if (inst == null) continue;

            if (inst.CurrentCooldown > 0f)
            {
                inst.TickCooldown(dt);
                if (inst.CurrentCooldown > 0f)
                    _cooldownsActiveCount++;
            }
        }

        if (_cooldownsActiveCount <= 0)
            SetProcess(false);
    }

    private void RegisterDefaultBehaviours()
    {
        RegisterBehaviourFactory(SpellBehaviourType.Projectile, () => new ProjectileSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Area, () => new AreaSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Beam, () => new BeamSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Dash, () => new DashSpellBehaviour());
        RegisterBehaviourFactory(SpellBehaviourType.Explosion, () => new ExplosionSpellBehaviour());
    }

    public void RegisterBehaviourFactory(SpellBehaviourType type, Func<ISpellBehaviour> factory)
    {
        _behaviourFactories[type] = factory;
        _behaviours.Remove(type);
    }

    public void EquipKit(ElementKitDefinition kit)
    {
        _equipped[SpellSlot.LeftHand] = kit.LeftHandSpell  != null ? new SpellInstance(kit.LeftHandSpell) : null;
        _equipped[SpellSlot.RightHand] = kit.RightHandSpell != null ? new SpellInstance(kit.RightHandSpell) : null;
        _equipped[SpellSlot.Dash] = kit.DashSpell != null ? new SpellInstance(kit.DashSpell) : null;
    }

    public SpellInstance GetInstance(SpellSlot slot)
        => _equipped.TryGetValue(slot, out var inst) ? inst : null;

    private ISpellBehaviour GetBehaviourFor(SpellDefinition def)
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

    // ========= PUBLIC API dla stanów gracza =========

    public bool BeginCast(SpellSlot slot)
    {
        if (_activeSessions.ContainsKey(slot))
            return true; // już trwa

        var inst = GetInstance(slot);
        if (inst == null) return false;

        var def = inst.Definition;

        // Charge: jeśli na cooldownie, nie startuj (żadnych indicatorów)
        if ((def.CastMode == SpellCastMode.ChargeRelease || def.CastMode == SpellCastMode.ChargeAuto) && !inst.CanCast)
            return false;

        // Dla reszty: jeśli ma cooldown i tryb wymaga instant castu od razu, po prostu fail
        if (!inst.CanCast && def.CastMode == SpellCastMode.Instant)
            return false;

        var behaviour = GetBehaviourFor(def);
        if (behaviour == null) return false;

        SpellCastContext ctx = BuildContext(slot, inst);
        if (ctx.Caster == null) return false;

        var session = new SpellCastSession(slot, inst, ctx, behaviour);
        _activeSessions[slot] = session;

        // Press hook (indicator, start beama, itd.)
        session.Press?.OnPressed(session.Ctx);

        // Start logic per castmode
        switch (def.CastMode)
        {
            case SpellCastMode.Instant:
                TryPerformCast(session, applyCooldown: true);
                EndCast(slot, CastEndReason.Released);
                break;

            case SpellCastMode.HoldRepeatCooldown:
                TryPerformCast(session, applyCooldown: true);
                break;

            case SpellCastMode.Channel:
                // start bez cooldownu, tickowany w UpdateCast
                TryPerformCast(session, applyCooldown: false);
                break;

            case SpellCastMode.ChargeRelease:
            case SpellCastMode.ChargeAuto:
                // tylko indicator + update; cast w release/auto
                break;
        }

        return true;
    }

    public void UpdateCast(SpellSlot slot, float dt)
    {
        if (!_activeSessions.TryGetValue(slot, out var s))
            return;

        // Aktualizacja aim tylko gdy sesja trwa
        RefreshAim(ref s.Ctx);

        switch (s.Def.CastMode)
        {
            case SpellCastMode.HoldRepeatCooldown:
                var canCast = TryPerformCast(s, applyCooldown: true);
                if (canCast)
                    EmitSignal(nameof(SpellRecasted), (int)slot);
                break;

            case SpellCastMode.Instant:
                // nic; instant powinien się skończyć w BeginCast
                break;

            case SpellCastMode.Channel:
                UpdateChannel(s, dt);
                break;

            case SpellCastMode.ChargeRelease:
                UpdateCharge(s, dt, autoCast: false);
                break;

            case SpellCastMode.ChargeAuto:
                UpdateCharge(s, dt, autoCast: true);
                break;
        }
    }

    public void EndCast(SpellSlot slot, CastEndReason reason)
    {
        if (!_activeSessions.TryGetValue(slot, out var s))
            return;

        _activeSessions.Remove(slot);

        switch (s.Def.CastMode)
        {
            case SpellCastMode.Channel:
                // kończymy emitter + cooldown
                s.Cancel?.Cancel(s.Ctx);
                ApplyCooldown(s.Instance);
                break;

            case SpellCastMode.ChargeRelease:
            case SpellCastMode.ChargeAuto:
                if (reason == CastEndReason.Released)
                    ReleaseCharge(s);
                else
                    s.Cancel?.Cancel(s.Ctx);
                break;
            default:
                // HoldRepeatCooldown/Instant: nic specjalnego
                break;
        }
    }

    // ========= IMPLEMENTACJA TRYBÓW =========

    private void UpdateChannel(SpellCastSession s, float dt)
    {
        if (s.Def is not ChannelSpellDefinition channelDef)
        {
            GD.PrintErr($"Spell {s.Def.Id} has CastMode=Channel but is not ChannelSpellDefinition.");
            return;
        }

        // beam/area: podążaj za aim
        s.Hold?.OnHeld(s.Ctx, dt);

        s.TickAcc += dt;
        float tickRate = Mathf.Max(0.01f, channelDef.TickRate);

        while (s.TickAcc >= tickRate)
        {
            s.TickAcc -= tickRate;
            TryPerformCast(s, applyCooldown: false);
        }
    }

    private void UpdateCharge(SpellCastSession s, float dt, bool autoCast)
    {
        float chargeTime = GetChargeTimeOrFallback(s.Def);

        s.ChargeTimeAcc += dt;
        s.Ctx.Charge01 = Mathf.Clamp(s.ChargeTimeAcc / Mathf.Max(0.01f, chargeTime), 0f, 1f);

        // indicator/update
        s.Hold?.OnHeld(s.Ctx, dt);

        if (autoCast && !s.HasAutoCasted && s.ChargeTimeAcc >= chargeTime)
        {
            ReleaseCharge(s);
            s.HasAutoCasted = true;
            // autocast kończy sesję automatycznie, bo i tak “puściłeś” w sensie gameplay
            EndCast(s.Slot, CastEndReason.Released);
        }
    }

    private bool ReleaseCharge(SpellCastSession s)
    {
        if (s.Release == null)
            return false;

        bool didCast = s.Release.OnReleased(s.Ctx);
        if (didCast)
            ApplyCooldown(s.Instance);

        return didCast;
    }

    private bool TryPerformCast(SpellCastSession s, bool applyCooldown)
    {
        if (!s.Instance.CanCast)
            return false;

        s.Behaviour.PerformCast(s.Ctx);

        if (applyCooldown)
            ApplyCooldown(s.Instance);

        return true;
    }

    public void ApplyCooldown(SpellInstance instance)
    {
        float cdr = StatsManager.GetStat(StatId.CooldownReduction);
        float cdrMult = 1f - cdr;

        bool wasZero = instance.CurrentCooldown <= 0f;
        instance.PutOnCooldown(cdrMult);

        if (wasZero && instance.CurrentCooldown > 0f)
        {
            _cooldownsActiveCount++;
            SetProcess(true);
        }
    }

    // ========= CONTEXT / AIM =========

    private SpellCastContext BuildContext(SpellSlot slot, SpellInstance instance)
    {
        var stats = instance.BuildCastStats(StatsManager);

        Node3D muzzle = slot switch
        {
            SpellSlot.LeftHand => _leftHand,
            SpellSlot.RightHand => _rightHand,
            SpellSlot.Dash => _caster,
            _ => _caster
        };

        Vector3 dir = GetAimDirection(muzzle);

        return new SpellCastContext
        {
            Caster = _caster,
            Muzzle = muzzle,
            Direction = dir,
            Slot = slot,
            Instance = instance,
            Stats = stats,
            Charge01 = 0f
        };
    }

    private void RefreshAim(ref SpellCastContext ctx)
    {
        if (ctx.Muzzle == null) return;
        ctx.Direction = GetAimDirection(ctx.Muzzle);
        // Stats zwykle nie zmieniasz co frame. Jeśli masz upgrade podczas castu, przebuduj tylko wtedy eventem.
    }

    private Vector3 GetAimDirection(Node3D muzzle)
    {
        if (_raycast != null && _raycast.IsColliding())
        {
            var hit = _raycast.GetCollisionPoint();
            var toHit = hit - muzzle.GlobalPosition;

            // sensownie: "dalej niż 3" -> squared > 9
            if (toHit.LengthSquared() > 9f)
                return toHit.Normalized();
        }

        var camPos = _camera.GlobalTransform.Origin;
        var camFwd = -_camera.GlobalTransform.Basis.Z.Normalized();

        var farPoint = camPos + camFwd * 1000f;
        return (farPoint - muzzle.GlobalPosition).Normalized();
    }

    private float GetChargeTimeOrFallback(SpellDefinition spellDef)
    {
        if (spellDef is ExplosionSpellDefinition explosion)
            return Mathf.Max(0.01f, explosion.ChargeTime);

        GD.PrintErr($"Spell {spellDef.Id} uses Charge mode but has no ChargeTime. Using fallback 0.75s.");
        return 0.75f;
    }

    public bool IsCooldownShort(SpellSlot slot)
    {
        var instance = GetInstance(slot);
        return instance != null && instance.Definition.BaseCooldown < 0.4f;
    }
}
