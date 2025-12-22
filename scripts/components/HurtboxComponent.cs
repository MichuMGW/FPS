using Godot;

// Bazowy komponent od przyjmowania obrażeń.
public abstract partial class HurtboxComponent : Node
{
    [Signal] public delegate void HitEventHandler(HitInfo hitInfo);

    [Export] public float RehitCooldownSeconds { get; set; } = 0f;
	private bool _active;
	public bool Active
    {
        get => _active;
		set
        {
            _active = value;
			SetHurtboxesMonitoring(value);
			SetHutboxesMonitorable(value);
        }
    }
    public void ReceiveHit(HurtboxArea hurtbox, Node3D source, Vector3 hitPosition)
    {
        ProcessHit(hurtbox, source, hitPosition);
    }
    protected void ProcessHit(HurtboxArea hurtbox, Node3D source, Vector3 hitPosition)
    {
        if (source is not IDamageSource damageSource)
            return;

        var rootTarget = GetOwner<Node3D>();
        if (rootTarget == null)
            return;

        if (!damageSource.CanHitAgain(rootTarget))
            return;

        damageSource.RegisterHit(rootTarget);

        var hitInfo = new HitInfo
        {
            Source = source,
            HitboxType = hurtbox.HurtboxType,
            DamageMultiplier = hurtbox.DamageMultiplier,
            HitPosition = hitPosition
        };

        EmitSignal(nameof(Hit), hitInfo);
    }

	public abstract void SetHurtboxesMonitoring(bool value);
	public abstract void SetHutboxesMonitorable(bool value);
}
