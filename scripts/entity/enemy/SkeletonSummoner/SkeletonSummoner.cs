using Godot;
using System;
using System.Collections.Generic;

public partial class SkeletonSummoner : CharacterBody3D
{
    [Export] public PackedScene SkeletonScene {get; private set;}
    [Export] public int QuantityToSpawn {get; set; } = 3;
    // rozmiar "bańki" sprawdzającej, czy nie wchodzimy w ścianę / innego moba
    [Export] public float SpawnCheckRadius { get; set; } = 0.6f;
    [Export] public float MinSummonRadius { get; set; } = 2.0f;
    [Export] public float MaxSummonRadius { get; set; } = 4.0f;
     // wysokość z której rzucamy raycast w dół
    [Export] public float RaycastHeight { get; set; } = 5.0f;
    public HealthComponent Health {get; private set; }
    public HurtboxComponent Hurtbox {get; private set; }
    public AnimationPlayer Animation {get; private set; }
    public float SummonCooldown {get; set; } = 5f;

    private IState _currentState;
    private Dictionary<SkeletonSummonerStateId, IState> _states;
    public SkeletonSummonerStateId CurrentStateId { get; private set; }
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    public override void _Ready()
    {
        FindNodes();
        SetAliveStateCollisions();
        Health.EntityDied += OnEntityDied;

        _states = new Dictionary<SkeletonSummonerStateId, IState>
        {
            { SkeletonSummonerStateId.Idle,  new SkeletonSummonerIdleState(this) },
            { SkeletonSummonerStateId.Summon, new SkeletonSummonerSummonState(this) },
            { SkeletonSummonerStateId.Dead,   new SkeletonSummonerDeadState(this) },
        };

        ChangeState(SkeletonSummonerStateId.Idle);
    }

    private void FindNodes()
    {
        Health = GetNode<HealthComponent>("HealthComponent");
        Hurtbox = Health.Hurtbox;
        
        Animation = GetNode<AnimationPlayer>("skeleton/AnimationPlayer");
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public void ChangeState(SkeletonSummonerStateId newState)
    {
        if (_currentState != null && CurrentStateId == newState)
            return;

        _currentState?.Exit();
        CurrentStateId = newState;
        _currentState = _states[newState];
        _currentState.Enter();
    }

    private void OnEntityDied()
    {
        SetDeadStateCollisions();
        ChangeState(SkeletonSummonerStateId.Dead);
    }

    public void SummonSkeletons()
    {
        if (SkeletonScene == null)
        {
            GD.PrintErr("MinionScene is null on SummonerEnemy!");
            return;
        }

        int countToSummon = QuantityToSpawn;
        int maxTriesPerMinion = 10;

        for (int i = 0; i < countToSummon; i++)
        {
            if (TryGetValidSummonPosition(out Vector3 spawnPos, maxTriesPerMinion))
            {
                SpawnSkeleton(spawnPos);
            }
            else
            {
                GD.Print($"[Summoner] Could not find valid position for minion #{i + 1}");
            }
        }
    }

    private void SpawnSkeleton(Vector3 position)
    {
        var skeletonInstance = SkeletonScene.Instantiate<CharacterBody3D>();
        GetTree().CurrentScene.AddChild(skeletonInstance);
        skeletonInstance.GlobalPosition = position;
    }

    private bool TryGetValidSummonPosition(out Vector3 spawnPosition, int maxTries)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        for (int i = 0; i < maxTries; i++)
        {
            // 1) losujemy kierunek na płaszczyźnie XZ
            float angle = _rng.RandfRange(0, Mathf.Tau);
            float radius = _rng.RandfRange(MinSummonRadius, MaxSummonRadius);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 roughPos = GlobalPosition + offset;

            // 2) raycast z góry w dół, żeby znaleźć prawdziwą ziemię (nierówna mapa)
            Vector3 from = roughPos + Vector3.Up * RaycastHeight;
            Vector3 to = roughPos + Vector3.Down * RaycastHeight * 2.0f;

            var rayParams = PhysicsRayQueryParameters3D.Create(from, to);
            rayParams.CollisionMask = PhysicsLayers.TERRAIN;

            var rayResult = spaceState.IntersectRay(rayParams);

            if (rayResult.Count == 0)
            {
                // nic nie trafiliśmy – pewnie w powietrzu / poza mapą
                continue;
            }

            Vector3 groundPos = (Vector3)rayResult["position"];

            // 3) sprawdzamy, czy w tym miejscu jest miejsce na moba
            if (!HasSpaceForSkeleton(groundPos))
            {
                continue;
            }

            spawnPosition = groundPos;
            return true;
        }

        spawnPosition = Vector3.Zero;
        return false;
    }

    private bool HasSpaceForSkeleton(Vector3 position)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        // prosty "bubble" check – kula o promieniu SpawnCheckRadius
        var sphereShape = new SphereShape3D
        {
            Radius = SpawnCheckRadius
        };

        var shapeParams = new PhysicsShapeQueryParameters3D
        {
            Shape = sphereShape,
            Transform = new Transform3D(Basis.Identity, position + Vector3.Up * SpawnCheckRadius),
            CollisionMask = PhysicsLayers.TERRAIN | PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY
        };

        // sprawdzamy, czy w tej bańce nie ma ścian / innych colliderów
        var intersections = spaceState.IntersectShape(shapeParams, 4); // max 4 wyniki, nieistotne

        // jeśli cokolwiek znaleźliśmy -> miejsce zajęte
        return intersections.Count == 0;
    }

    private void SetAliveStateCollisions()
    {
        CollisionLayer = PhysicsLayers.ENEMY_BODY;
        CollisionMask = PhysicsLayers.ENEMY_BODY | PhysicsLayers.PLAYER_BODY | PhysicsLayers.TERRAIN;
    }
    private void SetDeadStateCollisions()
    {
        CollisionMask = PhysicsLayers.TERRAIN;
    }



}