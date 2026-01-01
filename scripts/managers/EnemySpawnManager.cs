using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawnManager : Node3D
{
    [Export] public Vector2 MapHalfExtents = new(285f, 285f);

    [Export(PropertyHint.Range, "0,300,0.1")]
    public float MinSpawnDistance = 40f;

    [Export(PropertyHint.Range, "0,300,0.1")]
    public float MaxSpawnDistance = 50f;

    [Export(PropertyHint.Range, "0,180,1")]
    public float CameraFovDegrees = 120f;

    [Export] public uint TerrainCollisionMask = PhysicsLayers.TERRAIN;
    [Export] public uint ObstacleCollisionMask = PhysicsLayers.OBSTACLES;
    [Export] public float RaycastHeight = 100f;
    [Export] public float SpawnClearRadius = 1.5f;
    [Export] public int MaxTriesPerSpawn = 25;

    [Export] public Texture2D SpawnMaskTexture;
    [Export] public bool FlipV = true;

    private DifficultySnapshot _currentDifficulty;
    public DifficultySnapshot CurrentDifficulty => _currentDifficulty;

    private Node3D _player;
    private Camera3D _camera;
    public Node3D EnemiesRoot;
    public Node3D MinionsRoot;

    public int AliveEnemiesCount => EnemiesRoot?.GetChildCount() ?? 0;
    public int AliveMinionsCount => MinionsRoot?.GetChildCount() ?? 0;

    private Image _maskImage;
    private int _maskWidth;
    private int _maskHeight;

    private bool _spawningEnabled;

    private bool _navReady = false;
    private Rid _navMap;

    private sealed class EnemySpawnEntry
    {
        public PackedScene Scene;
        public float Weight;

        public EnemySpawnEntry(PackedScene scene, float weight)
        {
            Scene = scene;
            Weight = weight;
        }
    }

    private readonly List<EnemySpawnEntry> _enemyPool = new();

    private bool _weightsDirty = true;
    private float _totalWeight = 0f;

    public override void _Ready()
    {
        Init();
        InitMaskImage();
    }

    public void Init()
    {
        GD.Randomize();

        _player = GetTree().GetFirstNodeInGroup("player") as Node3D;

        if (_player != null && _camera == null)
            _camera = _player.GetNodeOrNull<Camera3D>("Head/Camera3D");

        if (EnemiesRoot == null)
            EnemiesRoot = this;

        var currentScene = GetTree().CurrentScene;
        if (currentScene != null)
        {
            EnemiesRoot = currentScene.GetNode<Node3D>("EnemiesRoot");
            MinionsRoot = currentScene.GetNode<Node3D>("MinionsRoot");
        }

        CallDeferred(nameof(InitNavigationMap));
    }

    private void InitNavigationMap()
    {
        var world3D = GetWorld3D();

        _navMap = world3D.NavigationMap;

        if (_navMap.IsValid)
        {
            _navReady = true;
        }
    }

    private void InitMaskImage()
    {
        _maskImage = SpawnMaskTexture.GetImage();
        if (_maskImage == null) return;

        _maskWidth  = _maskImage.GetWidth();
        _maskHeight = _maskImage.GetHeight();
    }

    public void StartSpawning()
    {
        _spawningEnabled = true;
    }

    public void StopSpawning() => _spawningEnabled = false;

    public void UpdateDifficulty(DifficultySnapshot snapshot) => _currentDifficulty = snapshot;

    public void AddEnemyType(PackedScene enemyScene, float weight = 1f)
    {
        if (enemyScene == null) return;

        weight = Mathf.Max(0f, weight);

        for (int i = 0; i < _enemyPool.Count; i++)
        {
            if (_enemyPool[i].Scene == enemyScene)
            {
                _enemyPool[i].Weight = weight;
                _weightsDirty = true;
                return;
            }
        }

        _enemyPool.Add(new EnemySpawnEntry(enemyScene, weight));
        _weightsDirty = true;
    }

    public void SetEnemyWeight(PackedScene enemyScene, float weight)
    {
        if (enemyScene == null)
            return;

        weight = Mathf.Max(0f, weight);

        for (int i = 0; i < _enemyPool.Count; i++)
        {
            if (_enemyPool[i].Scene == enemyScene)
            {
                _enemyPool[i].Weight = weight;
                _weightsDirty = true;
                return;
            }
        }
    }

    public bool TryGetSpawnPosition(out Vector3 spawnPos)
    {
        spawnPos = default;

        if (_player == null)
        {
            GD.PushError("[EnemySpawnManager] Missing Player");
            return false;
        }
        if (_camera == null)
        {
            GD.PushError("[EnemySpawnManager] Missing Camera");
            return false;
        }

        if (!_navReady || !_navMap.IsValid)
        {
            // navmesh jeszcze nie gotowy – lepiej się wycofać niż walić błędem
            GD.PushError("[EnemySpawnManager] NavMesh is not ready");
            return false;
        }

        if (_maskImage == null)
        {
            GD.PushWarning("[EnemySpawnManager] Mask image is null – spawns will ignore mask.");
        }

        var space = GetWorld3D().DirectSpaceState;

        for (int i = 0; i < MaxTriesPerSpawn; i++)
        {
            float angle = (float)GD.RandRange(0.0, Mathf.Tau);
            float distance = (float)GD.RandRange(MinSpawnDistance, MaxSpawnDistance);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            Vector3 candidateXZ = _player.GlobalPosition + offset;

            candidateXZ.X = Mathf.Clamp(candidateXZ.X, -MapHalfExtents.X, MapHalfExtents.X);
            candidateXZ.Z = Mathf.Clamp(candidateXZ.Z, -MapHalfExtents.Y, MapHalfExtents.Y);

            Vector3 navPoint = NavigationServer3D.MapGetClosestPoint(_navMap, candidateXZ);

            float distToPlayerXZ = new Vector2(
                navPoint.X - _player.GlobalPosition.X,
                navPoint.Z - _player.GlobalPosition.Z
            ).Length();

            if (distToPlayerXZ < MinSpawnDistance || distToPlayerXZ > MaxSpawnDistance)
                continue;

            Vector3 from = navPoint + Vector3.Up * RaycastHeight;
            Vector3 to = navPoint + Vector3.Down * RaycastHeight * 2f;

            var rayParams = PhysicsRayQueryParameters3D.Create(from, to);
            rayParams.CollisionMask = TerrainCollisionMask;

            var hit = space.IntersectRay(rayParams);
            if (hit.Count == 0)
                continue;

            Vector3 groundPos = (Vector3)hit["position"];
            Vector3 normal    = (Vector3)hit["normal"];

            if (normal.Y < 0.7f)
                continue;

            if (!IsAllowedByMask(groundPos))
                continue;

            if (IsInsidePlayerFov(groundPos))
                continue;

            if (!IsPointFree(groundPos, space))
                continue;

            spawnPos = groundPos;
            return true;
        }

        return false;
    }


    public void SpawnRandomEnemy()
    {
        if (!_spawningEnabled)
            return;

        if (_enemyPool.Count == 0)
        {
            GD.PushWarning("[EnemySpawnManager] No enemy types registered.");
            return;
        }

        int idx = PickWeightedIndex();
        if (idx < 0)
        {
            GD.PushWarning("[EnemySpawnManager] Total weight is 0. Nothing to spawn.");
            return;
        }

        SpawnEnemy(_enemyPool[idx].Scene);
    }

    public Node3D SpawnEnemy(PackedScene enemyScene)
    {
        if (enemyScene == null)
            return null;

        if (!TryGetSpawnPosition(out var pos))
            return null;

        var enemy = enemyScene.Instantiate<Node3D>();
        EnemiesRoot.AddChild(enemy);
        enemy.GlobalPosition = pos;
        enemy.LookAt(_player.GlobalPosition, Vector3.Up, true);

        if (enemy is IScalableEnemy scalable)
            scalable.ApplyDifficulty(_currentDifficulty);

        return enemy;
    }

    private int PickWeightedIndex()
    {
        RebuildWeightCacheIfNeeded();

        if (_totalWeight <= 0f)
            return -1;

        float roll = GD.Randf() * _totalWeight;
        float sum = 0f;

        for (int i = 0; i < _enemyPool.Count; i++)
        {
            float w = Mathf.Max(0f, _enemyPool[i].Weight);
            sum += w;
            if (roll <= sum)
                return i;
        }

        return _enemyPool.Count - 1;
    }

    private void RebuildWeightCacheIfNeeded()
    {
        if (!_weightsDirty)
            return;

        _totalWeight = 0f;

        for (int i = 0; i < _enemyPool.Count; i++)
            _totalWeight += Mathf.Max(0f, _enemyPool[i].Weight);

        _weightsDirty = false;
    }

    // ================== HELPERY ==================

    private bool IsAllowedByMask(Vector3 worldPos)
    {
        if (_maskImage == null)
            return true;

        float u = (worldPos.X + MapHalfExtents.X) / (MapHalfExtents.X * 2f);
        float v = (worldPos.Z + MapHalfExtents.Y) / (MapHalfExtents.Y * 2f);

        u = Mathf.Clamp(u, 0f, 1f);
        v = Mathf.Clamp(v, 0f, 1f);

        if (FlipV)
            v = 1f - v;

        int px = (int)(u * (_maskWidth - 1));
        int py = (int)(v * (_maskHeight - 1));

        Color color = _maskImage.GetPixel(px, py);
        return color.R > 0.5f;
    }

    private bool IsInsidePlayerFov(Vector3 worldPos)
    {
        Vector3 toTarget = worldPos - _player.GlobalPosition;
        toTarget.Y = 0f;

        if (toTarget.LengthSquared() < 0.001f)
            return true;

        toTarget = toTarget.Normalized();

        Vector3 forward = -_player.GlobalTransform.Basis.Z;
        forward.Y = 0f;
        forward = forward.Normalized();

        float dot = Mathf.Clamp(forward.Dot(toTarget), -1f, 1f);
        float angleDeg = Mathf.RadToDeg(Mathf.Acos(dot));

        return angleDeg <= CameraFovDegrees * 0.5f;
    }

    private bool IsPointFree(Vector3 pos, PhysicsDirectSpaceState3D space)
    {
        var shape = new SphereShape3D { Radius = SpawnClearRadius };

        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, pos),
            CollisionMask = ObstacleCollisionMask
        };

        var results = space.IntersectShape(query, maxResults: 8);
        return results.Count == 0;
    }
}
