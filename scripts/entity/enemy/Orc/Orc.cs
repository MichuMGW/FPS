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
    public Area3D ChargeHitbox { get; private set; }

    protected override void FindNodes()
    {
        base.FindNodes();

        Animation = GetNodeOrNull<AnimationPlayer>("orc/AnimationPlayer");
        ChargeHitbox = GetNodeOrNull<Area3D>("ChargeHitbox");
    }

    protected override void OnAfterReady()
    {
        if (VelocityComp != null)
            BaseAcceleration = VelocityComp.Acceleration;

        if (ChargeHitbox != null)
            ChargeHitbox.BodyEntered += OnChargeHitboxBodyEntered;

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

        Vector3 dir = player.GlobalPosition - GlobalPosition;
        dir.Y = 0.01f;
        dir = dir.Normalized();

        // Damage: albo HitboxComponent/Health gracza, albo jak masz inny system
        // player.Health.TakeDamage(ChargeDamage);

        player.Knockback.ApplyKnockback(dir * ChargeKnockbackForce);

        ChangeState(OrcStateId.Stop);
    }

    protected override void ApplyStatsToComponents(bool initialLoad)
    {
        base.ApplyStatsToComponents(initialLoad);

        ChaseSpeed = MoveSpeed;
    }
}
