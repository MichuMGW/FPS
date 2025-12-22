using Godot;

public partial class AreaEmitter : Node3D
{
    [Export] public NodePath HitboxPath = "Hitbox";
    [Export] public float ForwardOffset = 1.2f; // żeby było przed graczem

    private HitboxComponent hitbox;

    public override void _Ready()
    {
        hitbox = GetNodeOrNull<HitboxComponent>(HitboxPath);
        if (hitbox == null)
            GD.PrintErr(Name + ": AreaEmitter requires HitboxComponent at path: " + HitboxPath);
    }

    public void Configure(Element element, float damagePerTick, float tickRateSeconds)
    {
        if (hitbox == null)
            return;

        hitbox.Damage = damagePerTick;
        hitbox.DamageType = element;
        hitbox.OneShot = false;

        // to robi “tickrate” w twoim systemie
        hitbox.RehitCooldownSeconds = Mathf.Max(0.01f, tickRateSeconds);
    }

    public void FollowMuzzle(Node3D muzzle, Vector3 direction)
    {
        if (muzzle == null)
            return;

        GlobalTransform = muzzle.GlobalTransform;

        // wysuń przed wylot
        GlobalPosition += direction.Normalized() * ForwardOffset;

        // opcjonalnie: obrót w kierunku strzału
        LookAt(GlobalPosition + direction.Normalized(), Vector3.Up);
    }
}
