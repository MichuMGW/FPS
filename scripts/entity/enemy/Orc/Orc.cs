using Godot;
using System.Collections.Generic;

public partial class Orc : StateMachineEnemy<OrcStateId>
{
    // KONFIG
    [Export] public float ChaseSpeed { get; set; } = 4f;

    public float BaseAcceleration { get; private set; }

    [Export] public float ChargeAcceleration { get; set; } = 4f;
    [Export] public float ChargeCooldown { get; set; } = 3f;
    [Export] public float ChargeSpeed { get; set; } = 12f;
    [Export] public float ChargeTurnSpeed { get; set; } = 2f;
    [Export] public float MaxChargeTime { get; set; } = 2.5f;
    [Export] public float ObstacleCheckDistance { get; set; } = 3f;

    [Export] public float ChargeMinDistance { get; set; } = 6f;
    [Export] public float ChargeMaxDistance { get; set; } = 18f;
    [Export] public float ChargeFovDotThreshold { get; set; } = 0.5f;

    [Export] public float ChargeRampSpeed { get; private set; } = 2f;
    [Export] public float ChargeDamage { get; set; } = 20f;
    [Export] public float ChargeKnockbackForce { get; set; } = 15f;

    [Export] public uint LineOfSightMask { get; set; } = PhysicsLayers.TERRAIN | PhysicsLayers.PLAYER_BODY;
    [Export] public uint ObstacleMask { get; set; } = PhysicsLayers.TERRAIN;

    public AnimationPlayer Animation { get; private set; }
    public HitboxComponent ChargeHitbox { get; private set; }

    protected override void FindNodes()
    {
        base.FindNodes();

        Animation = GetNodeOrNull<AnimationPlayer>("orc/AnimationPlayer");
        ChargeHitbox = GetNodeOrNull<HitboxComponent>("HitboxComponent");
    }

    protected override void OnAfterReady()
    {
        if (VelocityComp != null)
            BaseAcceleration = VelocityComp.Acceleration;

        if (ChargeHitbox != null)
        {
            ChargeHitbox.BodyEntered += OnChargeHitboxBodyEntered;
            ChargeHitbox.Damage = Damage;
        }

        ChaseSpeed = MoveSpeed;
        ChargeSpeed = MoveSpeed * 1.5f;

        States = new Dictionary<OrcStateId, IState>
        {
            { OrcStateId.Chase, new OrcChaseState(this) },
            { OrcStateId.Charge, new OrcChargeState(this) },
            { OrcStateId.Stop, new OrcStopState(this) },
            { OrcStateId.Dead, new OrcDeadState(this) },
        };

        ChangeState(OrcStateId.Chase);
    }

    protected override void OnDied()
    {
        ChangeState(OrcStateId.Dead);
    }

    public override void _ExitTree()
    {
        if (ChargeHitbox != null)
            ChargeHitbox.BodyEntered -= OnChargeHitboxBodyEntered;

        base._ExitTree();
    }

    private void OnChargeHitboxBodyEntered(Node3D body)
    {
        if (body is not Player player)
            return;

        // Bez komponentu knockback nie ma co udawa 
        if (player.Knockback == null)
            return;

        // Kierunek: od orka do gracza (po ziemi)
        Vector3 dir = (player.GlobalPosition - GlobalPosition);
        dir.Y = 0f;

        if (dir.LengthSquared() < 0.0001f)
            dir = -GlobalTransform.Basis.Z;

        dir = dir.Normalized();

        Vector3 impulse = dir * ChargeKnockbackForce;
        impulse.Y = Mathf.Max(impulse.Y, 1.5f);

        player.StartKnockback(impulse);

        ChangeState(OrcStateId.Stop);
    }

    protected override void ApplyStatsToComponents(bool initialLoad)
    {
        base.ApplyStatsToComponents(initialLoad);

        ChaseSpeed = MoveSpeed;
    }
}
