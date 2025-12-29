using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawnManager : Node3D
{
    // ROZMIAR MAPY (ŚRODEK W (0,0), PROMIEŃ ~ 285 = 570/2)
    [Export] public Vector2 MapHalfExtents = new(285f, 285f);

    // PIERŚCIEŃ SPAWNU
    [Export(PropertyHint.Range, "0,300,0.1")]
    public float MinSpawnDistance = 30f;

    [Export(PropertyHint.Range, "0,300,0.1")]
    public float MaxSpawnDistance = 50f;

    // FOV (POZA TYM KĄTEM MOŻNA SPAWNIĆ)
    [Export(PropertyHint.Range, "0,180,1")]
    public float CameraFovDegrees = 120f;

    // KOLIZJE / TEREN
    [Export] public uint TerrainCollisionMask = PhysicsLayers.TERRAIN;
    [Export] public uint ObstacleCollisionMask = PhysicsLayers.OBSTACLES;
    [Export] public float RaycastHeight = 100f;
    [Export] public float SpawnClearRadius = 1.5f;
    [Export] public int MaxTriesPerSpawn = 25;

    // MASKA BW – BIAŁY = MOŻNA SPAWN
    [Export] public Texture2D SpawnMaskTexture;
    [Export] public bool FlipV = true; // jak maska będzie do góry nogami, flipniesz

    // DIFICULTY (od GameDirectora)
    private DifficultySnapshot _currentDifficulty;
    public DifficultySnapshot CurrentDifficulty => _currentDifficulty;

    // PODSTAWOWE NODY
    private Node3D _player;
    private Camera3D _camera;
    public Node3D EnemiesRoot;
    public Node3D MinionsRoot;

    public int AliveEnemiesCount => EnemiesRoot?.GetChildCount() ?? 0;
    public int AliveMinionsCount => MinionsRoot?.GetChildCount() ?? 0;

    // Maska
    private Image _maskImage;
    private int _maskWidth;
    private int _maskHeight;

    // Spawn type’y
    private readonly List<PackedScene> _enemyTypes = new();
    private bool _spawningEnabled;
    private float _spawnTimer;

    // Navi
    private bool _navReady = false;
    private Rid _navMap;

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

        // Jeżeli masz osobny node "EnemiesRoot" w scenie:
        // (jak nie masz, to możesz ten kawałek wywalić)
        var currentScene = GetTree().CurrentScene;
        EnemiesRoot = currentScene.GetNode<Node3D>("EnemiesRoot");
        MinionsRoot = currentScene.GetNode<Node3D>("MinionsRoot");

        CallDeferred(nameof(InitNavigationMap));
    }

    private void InitNavigationMap()
    {
        var world3D = GetWorld3D();
        if (world3D == null)
        {
            GD.PushError("[EnemySpawnManager] World3D is null in InitNavigationMap.");
            return;
        }

        _navMap = world3D.NavigationMap;

        if (_navMap.IsValid)
        {
            _navReady = true;
            GD.Print("[EnemySpawnManager] Navigation map is ready.");
        }
        else
        {
            GD.PushWarning("[EnemySpawnManager] Navigation map is not valid yet.");
        }
    }

    private void InitMaskImage()
    {
        if (SpawnMaskTexture == null)
        {
            GD.PushWarning("[EnemySpawnManager] SpawnMaskTexture not assigned – mask check disabled.");
            return;
        }

        _maskImage = SpawnMaskTexture.GetImage();
        if (_maskImage == null)
        {
            GD.PushError("[EnemySpawnManager] Could not get Image from SpawnMaskTexture.");
            return;
        }

        _maskWidth  = _maskImage.GetWidth();
        _maskHeight = _maskImage.GetHeight();
    }

    // ================== PUBLIC API ==================

    public void StartSpawning()
    {
        _spawningEnabled = true;
        _spawnTimer = 0f;
    }

    public void StopSpawning() => _spawningEnabled = false;

    public void UpdateDifficulty(DifficultySnapshot snapshot) => _currentDifficulty = snapshot;

    public void AddEnemyType(PackedScene enemyScene)
    {
        if (enemyScene == null)
        {
            GD.PushError("[EnemySpawnManager] Tried to add null scene.");
            return;
        }

        if (!_enemyTypes.Contains(enemyScene))
            _enemyTypes.Add(enemyScene);
    }

    // Proste API: spróbuj znaleźć pozycję – zwróć true/false
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
            // 1) losowy punkt 30–50m od gracza (XZ)
            float angle    = (float)GD.RandRange(0.0, Mathf.Tau);
            float distance = (float)GD.RandRange(MinSpawnDistance, MaxSpawnDistance);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            Vector3 candidateXZ = _player.GlobalPosition + offset;

            // Clamp do świata (pod maskę)
            candidateXZ.X = Mathf.Clamp(candidateXZ.X, -MapHalfExtents.X, MapHalfExtents.X);
            candidateXZ.Z = Mathf.Clamp(candidateXZ.Z, -MapHalfExtents.Y, MapHalfExtents.Y);

            // 2) przyciągnięcie do navmesha
            Vector3 navPoint = NavigationServer3D.MapGetClosestPoint(_navMap, candidateXZ);

            // sprawdź, czy dalej jesteśmy w pierścieniu
            float distToPlayerXZ = new Vector2(
                navPoint.X - _player.GlobalPosition.X,
                navPoint.Z - _player.GlobalPosition.Z
            ).Length();

            if (distToPlayerXZ < MinSpawnDistance || distToPlayerXZ > MaxSpawnDistance)
                continue;

            // 3) Raycast w dół – doprecyzowanie pozycji + normal
            Vector3 from = navPoint + Vector3.Up * RaycastHeight;
            Vector3 to   = navPoint + Vector3.Down * RaycastHeight * 2f;

            var rayParams = PhysicsRayQueryParameters3D.Create(from, to);
            rayParams.CollisionMask = TerrainCollisionMask;

            var hit = space.IntersectRay(rayParams);
            if (hit.Count == 0)
                continue;

            Vector3 groundPos = (Vector3)hit["position"];
            Vector3 normal    = (Vector3)hit["normal"];

            // Zbyt stromo?
            if (normal.Y < 0.7f)
                continue;

            // 4) Maska BW – biały = można spawnować
            if (!IsAllowedByMask(groundPos))
                continue;

            // 5) Poza FOV gracza
            if (IsInsidePlayerFov(groundPos))
                continue;

            // 6) Lokalna kolizja – czy coś tam już nie stoi
            if (IsPointFree(groundPos, space))
                continue;

            spawnPos = groundPos;
            return true;
        }

        return false;
    }

    // Losowy typ wroga z listy
    public void SpawnRandomEnemy()
    {
        if (_enemyTypes.Count == 0)
        {
            GD.PushWarning("[EnemySpawnManager] No enemy types registered.");
            return;
        }

        int idx = (int)GD.RandRange(0, _enemyTypes.Count);
        if (idx >= _enemyTypes.Count)
            idx = _enemyTypes.Count - 1;

        var scene = _enemyTypes[idx];
        SpawnEnemy(scene);
    }

    // Wygodne API: podaj scenę, ja znajdę punkt i zrespię
    public Node3D SpawnEnemy(PackedScene enemyScene)
    {
        if (!TryGetSpawnPosition(out var pos))
        {
            return null;
        }

        var enemy = enemyScene.Instantiate<Node3D>();
        EnemiesRoot.AddChild(enemy);
        enemy.GlobalPosition = pos;
        enemy.LookAt(_player.GlobalPosition, Vector3.Up, true);

        if (enemy is IScalableEnemy scalable)
        {
            scalable.ApplyDifficulty(_currentDifficulty);
            GD.Print("Current Difficulty Coeff: " + _currentDifficulty.Coeff); 
        }
            
        return enemy;
    }

    // ================== HELPERY ==================

    private bool IsAllowedByMask(Vector3 worldPos)
    {
        if (_maskImage == null)
            return true; // maska wyłączona → nie blokujemy

        // world XZ → UV 0..1
        float u = (worldPos.X + MapHalfExtents.X) / (MapHalfExtents.X * 2f);
        float v = (worldPos.Z + MapHalfExtents.Y) / (MapHalfExtents.Y * 2f);

        u = Mathf.Clamp(u, 0f, 1f);
        v = Mathf.Clamp(v, 0f, 1f);

        if (FlipV)
            v = 1f - v;

        int px = (int)(u * (_maskWidth  - 1));
        int py = (int)(v * (_maskHeight - 1));

        Color color = _maskImage.GetPixel(px, py);

        // B/W: biały ≈ 1, czarny ≈ 0. Wystarczy próg.
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

        float dot = forward.Dot(toTarget);
        dot = Mathf.Clamp(dot, -1f, 1f);

        float angleDeg = Mathf.RadToDeg(Mathf.Acos(dot));

        // jeśli punkt jest w stożku FOV – NIE jest ok do spawnu
        return angleDeg <= CameraFovDegrees * 0.5f;
    }

    private bool IsPointFree(Vector3 pos, PhysicsDirectSpaceState3D space)
    {
        var shape = new SphereShape3D { Radius = SpawnClearRadius };

        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, pos),
            CollisionMask = TerrainCollisionMask | ObstacleCollisionMask
        };

        var results = space.IntersectShape(query, maxResults: 8);
        return results.Count == 0;
    }
}
