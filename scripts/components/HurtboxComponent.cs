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

        float baseDamage = damageSource.GetDamage();

        float critChance = damageSource.GetCritChance();
        float critMultiplier = damageSource.GetCritMultiplier();

        bool isCrit = false;
        float finalDamage = baseDamage;

        // LOSOWANIE CRITA – per HIT
        if (critChance > 0f && GD.Randf() < critChance)
        {
            isCrit = true;
            finalDamage *= Mathf.Max(1f, critMultiplier);
        }

        // hurtbox multiplier (np. headshot)
        finalDamage *= hurtbox.DamageMultiplier;

        var hitInfo = new HitInfo(source, hurtbox.HurtboxType, hurtbox.DamageMultiplier, hitPosition)
        {
            BaseDamage = baseDamage,
            FinalDamage = finalDamage,
            IsCrit = isCrit,

            Element = damageSource.GetDamageType(),
            StatusProfile = damageSource.GetStatusProfile(),
            BurningDotMultiplier = damageSource.GetBurningDotMultiplier(),
            BleedDotMultiplier = damageSource.GetBleedDotMultiplier(),
            SlowBonus = damageSource.GetSlowBonus(),
            EarthBuildupPerHit = damageSource.GetEarthBuildupPerHit(),
        };

        EmitSignal(nameof(Hit), hitInfo);
    }

    protected void ProcessHit(HurtboxArea hurtbox, Node3D source) => ProcessHit(hurtbox, source, hurtbox.GlobalPosition);
	public abstract void SetHurtboxesMonitoring(bool value);
	public abstract void SetHutboxesMonitorable(bool value);
    public abstract void SetHurtboxAreaOwner(HurtboxArea owner);
}
