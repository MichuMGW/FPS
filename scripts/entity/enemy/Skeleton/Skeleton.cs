using Godot;

public partial class Skeleton : StateMachineEnemy<SkeletonStateId>
{
    public float AttackDistance { get; private set; } = 2f;
    public HitboxComponent Hitbox { get; private set; }

    public AnimationPlayer Animation { get; private set; }
    private RandomNumberGenerator _rng = new();

    protected override void FindNodes()
    {
        base.FindNodes();
        Animation = GetNodeOrNull<AnimationPlayer>("skeleton/AnimationPlayer");
        Hitbox = GetNodeOrNull<HitboxComponent>("HitboxComponent");
    }

    protected override void OnAfterReady()
    {
        if (Hitbox != null)
        {
            Hitbox.Active = false;
            Hitbox.Damage = Damage;
        }

        States = new()
        {
            { SkeletonStateId.Spawn, new SkeletonSpawnState(this) },
            { SkeletonStateId.Chase, new SkeletonChaseState(this) },
            { SkeletonStateId.Attack, new SkeletonAttackState(this) },
            { SkeletonStateId.Dead, new SkeletonDeadState(this) },
        };

        // ChangeState(SkeletonStateId.Spawn);
        ChangeState(SkeletonStateId.Spawn);
    }

    protected override void OnDied()
    {
        ChangeState(SkeletonStateId.Dead);
    }

    public void PlayAnimationRandomized(StringName animName, bool randomizeTime = false)
    {
        if (Animation == null || !Animation.HasAnimation(animName))
            return;

        Animation.Play(animName, 0.3f);

        float length = Animation.GetAnimation(animName).Length;
        if (randomizeTime)
            Animation.Seek(_rng.RandfRange(0f, length), true);

        Animation.SpeedScale = _rng.RandfRange(0.9f, 1.1f);
    }
}
