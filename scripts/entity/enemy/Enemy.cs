using Godot;

public abstract partial class Enemy : CharacterBody3D, IScalableEnemy
{
    [Export] public EnemyStatsResource StatsResource;

    public VelocityComponent VelocityComp { get; private set; }
    public PathfindComponent Pathfind { get; private set; }
    public HealthComponent Health { get; private set; }
    public HurtboxComponent Hurtbox { get; private set; }
    public DifficultySnapshot CurrentDifficulty { get; private set; }

    public Node3D Player { get; private set; }
    public Node3D PlayerAimTarget { get; private set; }

    public float MaxHealth { get; protected set; }
    public float MoveSpeed { get; protected set; }
    public float Damage { get; protected set; }

    private float _baseMaxHealth;
    private float _baseMoveSpeed;
    private float _baseDamage;

    private bool _alive = true;

    private GameEvents _events;

    public override void _Ready()
    {
        FindNodes();
        SetupAliveCollisions();
        WireSignals();

        LoadBaseStats();
        OnAfterReady();

        VelocityComp?.SetPhysicsProcess(false);
        Pathfind?.SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_alive) return;
        float dt = (float)delta;

        TickBrain(dt);

        if (Pathfind != null)
            Pathfind.Tick(dt);

        if (VelocityComp != null)
            VelocityComp.Tick(dt);
    }

    protected virtual void TickBrain(float dt) { }

    protected virtual void FindNodes()
    {
        VelocityComp = GetNodeOrNull<VelocityComponent>("VelocityComponent");
        Pathfind = GetNodeOrNull<PathfindComponent>("PathfindComponent");
        Health = GetNodeOrNull<HealthComponent>("HealthComponent");
        
        if (Health != null)
            Hurtbox = Health.Hurtbox;

        //Hitbox = GetNodeOrNull<HitboxComponent>("HitboxComponent");

        Player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        PlayerAimTarget = GetTree().GetFirstNodeInGroup("player_target") as Node3D;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
    }

    protected virtual void SetupAliveCollisions()
    {
        CollisionLayer = PhysicsLayers.ENEMY_BODY;
        CollisionMask = PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY | PhysicsLayers.TERRAIN;
    }

    protected virtual void SetupDeadCollisions()
    {
        CollisionMask = PhysicsLayers.TERRAIN;
        CollisionLayer = PhysicsLayers.NONE;
    }

    protected virtual void WireSignals()
    {
        if (Health != null)
            Health.EntityDied += OnDiedCommon;
    }

    private void OnDiedCommon()
    {
        _alive = false;
        SetupDeadCollisions();

        _events.EmitEnemyDied();

        if (Pathfind != null) Pathfind.Active = false;
        if (VelocityComp != null) VelocityComp.Active = false;

        OnDied();
    }

    protected virtual void OnAfterReady() { }
    protected virtual void OnDied() { }

    private void LoadBaseStats()
    {
        if (StatsResource == null)
        {
            GD.PushError($"[{Name}] Missing StatsResource. Using defaults.");
            _baseMaxHealth = 100f;
            _baseMoveSpeed = 5f;
            _baseDamage = 10f;
        }
        else
        {
            _baseMaxHealth = StatsResource.BaseMaxHealth;
            _baseMoveSpeed = StatsResource.BaseMoveSpeed;
            _baseDamage = StatsResource.BaseDamage;
        }

        MaxHealth = _baseMaxHealth;
        MoveSpeed = _baseMoveSpeed;
        Damage = _baseDamage;

        ApplyStatsToComponents(true);
    }

    public void ApplyDifficulty(DifficultySnapshot difficulty)
    {
        CurrentDifficulty = difficulty;
        MaxHealth = _baseMaxHealth * difficulty.HpMultiplier;
        MoveSpeed = _baseMoveSpeed * difficulty.MoveSpeedMultiplier;
        Damage = _baseDamage * difficulty.DamageMultiplier;
        ApplyStatsToComponents(false);
    }

    protected virtual void ApplyStatsToComponents(bool initialLoad)
    {
        if (Health != null)
        {
            Health.MaxHealth = MaxHealth;
            if (initialLoad || Health.CurrentHealth <= 0f) Health.CurrentHealth = MaxHealth;
            else Health.CurrentHealth = Mathf.Min(Health.CurrentHealth, MaxHealth);
        }

        if (VelocityComp != null) VelocityComp.MaxSpeed = MoveSpeed;
        if (Hitbox != null) Hitbox.Damage = Damage;
    }

    public void EnableMovement(bool enable)
    {
        if (VelocityComp != null) VelocityComp.Active = enable;
        if (Pathfind != null) Pathfind.Active = enable;
    }
}
