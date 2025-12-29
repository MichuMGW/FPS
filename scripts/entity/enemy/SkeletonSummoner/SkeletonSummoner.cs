using Godot;
using System.Collections.Generic;

public partial class SkeletonSummoner : StateMachineEnemy<SkeletonSummonerStateId>
{
    [Export] public PackedScene SkeletonScene { get; private set; }
    [Export] public int QuantityToSpawn { get; set; } = 3;

    [Export] public float SpawnCheckRadius { get; set; } = 0.5f;
    [Export] public float MinSummonRadius { get; set; } = 1.0f;
    [Export] public float MaxSummonRadius { get; set; } = 4.0f;
    [Export] public float RaycastHeight { get; set; } = 5.0f;

    [Export] public float SummonCooldown { get; set; } = 10f;

    public AnimationPlayer Animation { get; private set; }
    private RandomNumberGenerator _rng = new();
    private EnemySpawnManager _spawnManager;

    protected override void FindNodes()
    {
        base.FindNodes();
        Animation = GetNodeOrNull<AnimationPlayer>("skeleton/AnimationPlayer");
        _spawnManager = GetTree().CurrentScene.GetNodeOrNull<EnemySpawnManager>("EnemySpawnManager");
    }

    protected override void OnAfterReady()
    {
        States = new Dictionary<SkeletonSummonerStateId, IState>
        {
            { SkeletonSummonerStateId.Idle, new SkeletonSummonerIdleState(this) },
            { SkeletonSummonerStateId.Summon, new SkeletonSummonerSummonState(this) },
            { SkeletonSummonerStateId.Dead, new SkeletonSummonerDeadState(this) },
        };

        ChangeState(SkeletonSummonerStateId.Idle);
    }

    protected override void OnDied()
    {
        ChangeState(SkeletonSummonerStateId.Dead);
    }

    // Summoner nie ma VelocityComp ani Hitbox, więc baza i tak jest bezpieczna,
    // ale tu możesz jawnie zostawić tylko to co ma sens:
    protected override void ApplyStatsToComponents(bool initialLoad)
    {
        if (Health != null)
        {
            Health.MaxHealth = MaxHealth;

            if (initialLoad || Health.CurrentHealth <= 0f)
                Health.CurrentHealth = MaxHealth;
            else
                Health.CurrentHealth = Mathf.Min(Health.CurrentHealth, MaxHealth);
        }

    }

    public void SummonSkeletons()
    {
        if (SkeletonScene == null)
        {
            GD.PrintErr("[SkeletonSummoner] SkeletonScene is null");
            return;
        }

        var diff = _spawnManager.CurrentDifficulty;
        if (_spawnManager.AliveMinionsCount >= diff.MaxMinionOverCap)
        {
            return;
        }

        var maxTriesPerMinion = 10;
        var quantity = Mathf.Min(QuantityToSpawn, diff.MaxMinionOverCap - _spawnManager.AliveMinionsCount);

        for (int i = 0; i < quantity; i++)
        {
            if (TryGetValidSummonPosition(out Vector3 spawnPos, maxTriesPerMinion))
                SpawnSkeleton(spawnPos);
        }
    }

    private void SpawnSkeleton(Vector3 position)
    {
        // ważne: instantiate typowo jako Node3D/CharacterBody3D, ale potem próbujemy IScalableEnemy
        var skeletonInstance = SkeletonScene.Instantiate<Node3D>();
        _spawnManager.MinionsRoot.AddChild(skeletonInstance);
        skeletonInstance.GlobalPosition = position;

        if (skeletonInstance is IScalableEnemy scalable)
        {
            scalable.ApplyDifficulty(_spawnManager.CurrentDifficulty);
        }
    }

    private bool TryGetValidSummonPosition(out Vector3 spawnPosition, int maxTries)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        for (int i = 0; i < maxTries; i++)
        {
            float angle = _rng.RandfRange(0f, Mathf.Tau);
            float radius = _rng.RandfRange(MinSummonRadius, MaxSummonRadius);

            Vector3 offset = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            offset *= radius;

            Vector3 roughPos = GlobalPosition + offset;

            Vector3 from = roughPos + Vector3.Up * RaycastHeight;
            Vector3 to = roughPos + Vector3.Down * RaycastHeight * 2f;

            var rayParams = PhysicsRayQueryParameters3D.Create(from, to);
            rayParams.CollisionMask = PhysicsLayers.TERRAIN;

            var rayResult = spaceState.IntersectRay(rayParams);
            if (rayResult.Count == 0)
                continue;

            Vector3 groundPos = (Vector3)rayResult["position"];

            if (!HasSpaceForSkeleton(groundPos))
                continue;

            spawnPosition = groundPos;
            GD.Print("[SkeletonSummoner] Found valid summon position at " + spawnPosition);
            return true;
        }
        GD.Print("[SkeletonSummoner] Failed to find valid summon position");
        spawnPosition = Vector3.Zero;
        return false;
    }

    private bool HasSpaceForSkeleton(Vector3 position)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        var sphereShape = new SphereShape3D
        {
            Radius = SpawnCheckRadius
        };

        var shapeParams = new PhysicsShapeQueryParameters3D
        {
            Shape = sphereShape,
            Transform = new Transform3D(Basis.Identity, position + Vector3.Up * SpawnCheckRadius),
            CollisionMask = PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY
        };

        var intersections = spaceState.IntersectShape(shapeParams, 4);
        return intersections.Count == 0;
    }
}
