using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BeamEmitter : Node3D
{
    [Export] public NodePath HitboxPath = "HitboxComponent";
    [Export] public NodePath HitboxShapePath = "HitboxComponent/CollisionShape3D";
    [Export] public NodePath RaycastPath = "RayCast3D";
    [Export] public NodePath BeamVfxPath = "BeamVFX";
    [Export] public NodePath BeamMeshPath = "BeamVFX/BeamMesh";
    [Export] public NodePath OverlayMeshPath = "BeamVFX/OverlayMesh";

    // Jeśli chcesz, żeby beam skracał się na ścianach/terenie.
    [Export] public bool StopOnWorldCollision = true;
    [Export] public uint WorldCollisionMask = 0; // ustaw w inspektorze (np. Terrain|Obstacles)

    [Export] public float MaxLength = 20f;

    private HitboxComponent _hitbox;
    private CollisionShape3D _hitShape;
    private RayCast3D _raycast;

    private Node3D _beamVfx;
    private MeshInstance3D _beamMesh;
    private MeshInstance3D _overlayMesh;

    private ShaderMaterial _beamMat;
    private ShaderMaterial _overlayMat;

    private float _tickRate = 0.1f;
    private float _tickAcc = 0f;

    private float _beamWidth = 0.3f;
    private int _pierceCount = 999999; // domyślnie “ile wejdzie”, ograniczysz definicją

    private Vector2 _scaleParam = new(1f, 1f);
    private Vector2 _speedParam = new(0f, 1f);

    private bool _running = false;

    public override void _Ready()
    {
        _hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
        _hitShape = GetNodeOrNull<CollisionShape3D>(HitboxShapePath);
        _raycast = GetNodeOrNull<RayCast3D>(RaycastPath);

        _beamVfx = GetNodeOrNull<Node3D>(BeamVfxPath);
        _beamMesh = GetNodeOrNull<MeshInstance3D>(BeamMeshPath);
        _overlayMesh = GetNodeOrNull<MeshInstance3D>(OverlayMeshPath);

        _beamMat = _beamMesh?.GetActiveMaterial(0) as ShaderMaterial;
        _overlayMat = _overlayMesh?.GetActiveMaterial(0) as ShaderMaterial;

        if (_hitbox == null) GD.PushError($"{Name}: missing HitboxComponent at {HitboxPath}");
        if (_hitShape == null) GD.PushError($"{Name}: missing CollisionShape3D at {HitboxShapePath}");
        if (_beamVfx == null) GD.PushError($"{Name}: missing BeamVFX at {BeamVfxPath}");

        // Beam ma sens tylko gdy hitbox monitoruje.
        if (_hitbox != null)
            _hitbox.Active = true;
            
        if (_raycast != null)
            _raycast.TargetPosition = new Vector3(0, 0, MaxLength);
    }

    public override void _ExitTree()
    {
        _running = false;
    }

    /// <summary>
    /// Konfiguracja z definicji spella.
    /// damagePerTick: obrażenia zadawane na jeden tick.
    /// tickRateSeconds: odstęp między tickami.
    /// </summary>
    public void Configure(
        Element element,
        float damagePerTick,
        float tickRateSeconds,
        float maxLength,
        float beamWidth,
        float critChance,
        float critMultiplier,
        int pierceCount = 999999,
        ElementStatusProfile statusProfile = null,
        float burningDotMultiplier = 1f,
        float bleedDotMultiplier = 1f,
        float slowMultiplierBonus = 1f,
        float earthBuildupPerHit = 0f
    )
    {
        _tickRate = Mathf.Max(0.01f, tickRateSeconds);
        MaxLength = Mathf.Max(0.1f, maxLength);
        _beamWidth = Mathf.Max(0.01f, beamWidth);
        _pierceCount = Mathf.Max(1, pierceCount);

        if (_hitbox != null)
        {
            _hitbox.CritChance = critChance;
            _hitbox.CritMultiplier = critMultiplier;
            _hitbox.Damage = damagePerTick;
            _hitbox.DamageType = element;
            _hitbox.OneShot = false;
            _hitbox.RehitCooldownSeconds = _tickRate; // ważne: rehit = tick
            _hitbox.StatusProfile = statusProfile;          // dopnij do SpellDefinition
            _hitbox.BurningDotMultiplier = burningDotMultiplier;    // dopnij do SpellDefinition
            _hitbox.BleedDotMultiplier = bleedDotMultiplier;
            _hitbox.SlowBonus = slowMultiplierBonus;                // dopnij do SpellDefinition
            _hitbox.EarthBuildupPerHit = earthBuildupPerHit;        // dopnij do SpellDefinition
        }

        UpdateWidth(_beamWidth);

        SetInitialShaderParams();
        ApplyShaderParams();
    }

    private void SetInitialShaderParams()
    {
        const float baseLength = 2.0f;

        const float baseScale = 0.2f;
        const float baseSpeed = 0.5f;

        float lengthFactor = MaxLength / baseLength;

        _scaleParam = new Vector2(
            baseScale,
            baseScale * lengthFactor
        );

        _speedParam = new Vector2(
            0.1f,
            baseSpeed * lengthFactor
        );
    }

    public void UpdateBeam(Node3D muzzle, Vector3 direction)
    {
        if (muzzle == null) return;
        if (direction.LengthSquared() < 0.0001f) return;

        var dir = direction.Normalized();

        GlobalPosition = muzzle.GlobalPosition;
        GlobalBasis = BasisFromForwardPlusZ(dir);

        float length = ComputeLength();
        UpdateVfxLength(length);
        UpdateHitboxLength(length);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_running) return;
        if (_hitbox == null) return;
        if (!_hitbox.Monitoring) return;

        _tickAcc += (float)delta;
        while (_tickAcc >= _tickRate)
        {
            _tickAcc -= _tickRate;
            ApplyDamageTick();
        }
    }

    public void Start()
    {
        _running = true;
        _tickAcc = 0f;
    }

    public void Stop()
    {
        _running = false;
    }

    private float ComputeLength()
    {
        if (_raycast == null || !_raycast.Enabled)
            return MaxLength;

        _raycast.TargetPosition = new Vector3(0, 0, MaxLength);
        _raycast.ForceRaycastUpdate();

        if (!_raycast.IsColliding())
            return MaxLength;

        var hitPos = _raycast.GetCollisionPoint();
        float dist = GlobalPosition.DistanceTo(hitPos);

        return Mathf.Clamp(dist, 0.1f, MaxLength);
    }


    private void ApplyDamageTick()
    {
        // Godot: GetOverlappingAreas działa tylko gdy monitoring włączony i coś weszło w broadphase.
        var overlapped = _hitbox.GetOverlappingAreas();
        if (overlapped == null || overlapped.Count == 0)
            return;

        // Zbieramy HurtboxArea, potem sortujemy po odległości od początku beama (po lokalnym Z).
        var candidates = new List<HurtboxArea>(overlapped.Count);
        foreach (var a in overlapped)
        {
            if (a is HurtboxArea ha && ha.OwnerHurtboxComponent != null)
                candidates.Add(ha);
        }

        if (candidates.Count == 0)
            return;

        // Ograniczenie “pierce” = ile celów na tick.
        // Sort: kto jest “bliżej” wzdłuż wiązki (projekcja na forward).
        Vector3 origin = GlobalPosition;
        Vector3 forward = GlobalBasis.Z.Normalized();

        candidates.Sort((a, b) =>
        {
            float da = (a.GlobalPosition - origin).Dot(forward);
            float db = (b.GlobalPosition - origin).Dot(forward);
            return da.CompareTo(db);
        });

        int hitsLeft = _pierceCount;
        foreach (var hurtboxArea in candidates)
        {
            if (hitsLeft-- <= 0)
                break;

            // HitPosition: punkt na hurtboxie (na start wystarczy jego GlobalPosition)
            hurtboxArea.OwnerHurtboxComponent.ReceiveHit(hurtboxArea, _hitbox, hurtboxArea.GlobalPosition);
        }
    }

    private static Basis BasisFromForwardPlusZ(Vector3 forward)
    {
        var z = forward.Normalized();
        var up = Vector3.Up;
        if (Mathf.Abs(z.Dot(up)) > 0.98f) up = Vector3.Right;

        var x = up.Cross(z).Normalized();
        var y = z.Cross(x).Normalized();
        return new Basis(x, y, z); // +Z = forward
    }

    private void UpdateHitboxLength(float length)
    {
        if (_hitShape.Shape is CylinderShape3D cyl)
        {
            cyl.Radius = _beamWidth * 0.5f;
            cyl.Height = length;
            _hitShape.Position = new Vector3(0, length * 0.5f, 0);
        }
    }

    private void UpdateVfxLength(float length)
    {
        if (_beamMesh != null)
        {
            var s = _beamMesh.Scale;
            s.Z = length;
            _beamMesh.Scale = s;

        }

        if (_overlayMesh != null)
        {
            var s2 = _overlayMesh.Scale;
            s2.Z = length;
            _overlayMesh.Scale = s2;

            
        }

        _scaleParam.Y = length;
        ApplyShaderParams();
    }

    private void UpdateWidth(float width)
    {
        if (_beamMesh != null)
        {
            var s = _beamMesh.Scale;
            s.X = width - 0.01f;
            s.Y = width - 0.01f;
            _beamMesh.Scale = s;
        }

        if (_overlayMesh != null)
        {
            var s2 = _overlayMesh.Scale;
            s2.X = width;
            s2.Y = width;
            _overlayMesh.Scale = s2;
        }

        if (_hitShape?.Shape is BoxShape3D box)
        {
            var size = box.Size;
            size.X = width;
            size.Y = width;
            box.Size = size;
        }

        if (_hitShape?.Shape is CylinderShape3D cyl)
        {
            cyl.Radius = width * 0.5f;
        }
    }

    private void ApplyShaderParams()
    {
        _overlayMat?.SetShaderParameter("Scale", _scaleParam);
        _overlayMat?.SetShaderParameter("Speed", _speedParam);
    }

}
