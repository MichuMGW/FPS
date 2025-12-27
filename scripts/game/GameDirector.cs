using Godot;
using System;
using System.Collections.Generic;

public partial class GameDirector : Node
{
    [Export] public float MatchDurationSeconds { get; set; } = 600f; // 10 minut
    [Export] public NodePath EnemySpawnManagerPath { get; set; }
    [Export] public ElementKitDatabase KitDatabase;
    [Export] private PackedScene _elementOverlayScene;

    private EnemySpawnManager _enemySpawnManager;
    private DifficultyManager _difficultyManager = new DifficultyManager();

    private GameEvents _events;
    private RunElementState _runState;
    private PlayerSpellController _spellController;

    private float _elapsed;
    private float _spawnTimer;
    private bool _isRunning;

    // Konfiguracja odblokowywania typów przeciwników
    private readonly List<EnemyUnlockConfig> _enemyUnlocks = new()
    {
        // czas w sekundach, ścieżka do sceny wroga
        new EnemyUnlockConfig(   0f, "res://scenes/entities/enemies/Skeleton.tscn"),
        new EnemyUnlockConfig(  60f, "res://scenes/entities/enemies/TrollArcher.tscn"),
        new EnemyUnlockConfig( 120f, "res://scenes/entities/enemies/Orc.tscn"),
        new EnemyUnlockConfig( 180f, "res://scenes/entities/enemies/SkeletonSummoner.tscn"),
        // dodajesz kolejne według potrzeb
    };

    public override void _Ready()
    {
        if (EnemySpawnManagerPath == null || EnemySpawnManagerPath.IsEmpty)
        {
            GD.PushError("[GameDirector] EnemySpawnManagerPath is not set.");
            return;
        }

        _enemySpawnManager = GetNode<EnemySpawnManager>(EnemySpawnManagerPath);

        if (_enemySpawnManager == null)
        {
            GD.PushError("[GameDirector] Could not find EnemySpawnManager.");
            return;
        }

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _runState = GetTree().Root.GetNodeOrNull<RunElementState>("RunElementState");
        _spellController = GetTree().GetFirstNodeInGroup("player")?.GetNodeOrNull<PlayerSpellController>("PlayerSpellController")
                          ?? GetTree().GetFirstNodeInGroup("player") as PlayerSpellController;

        if (_events != null)
            _events.ElementPicked += OnElementPicked;

        _elapsed = 0f;
        _spawnTimer = 0f;
        _isRunning = true;

        // na starcie – od razu odblokujemy te typy, które mają UnlockTime <= 0
        UnlockEnemiesIfNeeded();
    }

    public override void _Process(double delta)
    {
        if (!_isRunning)
            return;

        _elapsed += (float)delta;

        // koniec meczu po 10 minutach
        if (_elapsed >= MatchDurationSeconds)
        {
            EndMatch();
            return;
        }

        float normalizedTime = _elapsed / MatchDurationSeconds;

        // 1) liczymy difficulty z managera
        DifficultySnapshot diff = _difficultyManager.GetDifficulty(normalizedTime, _elapsed);

        // 2) przekazujemy do SpawnManagera – będzie użyte przy każdym nowym spawnie
        _enemySpawnManager.UpdateDifficulty(diff);

        // 3) sprawdzamy, czy trzeba odblokować nowe typy wrogów
        UnlockEnemiesIfNeeded();

        // jeśli nadal nie ma żadnego typu wroga – nie spawnujemy
        if (_enemySpawnManager.AliveEnemiesCount >= diff.MaxAliveEnemies)
            return;

        // 4) obsługa timera spawnu
        _spawnTimer -= (float)delta;
        if (_spawnTimer <= 0f)
        {
            // robimy spawn
            _enemySpawnManager.SpawnRandomEnemy();

            // reset timera na podstawie difficulty
            _spawnTimer = diff.SpawnInterval;
        }
    }

    private void UnlockEnemiesIfNeeded()
    {
        foreach (var unlock in _enemyUnlocks)
        {
            if (!unlock.Unlocked && _elapsed >= unlock.UnlockTimeSeconds)
            {
                unlock.Unlocked = true;

                var scene = GD.Load<PackedScene>(unlock.ScenePath);
                if (scene == null)
                {
                    GD.PushError($"[GameDirector] Cannot load enemy scene at path: {unlock.ScenePath}");
                    continue;
                }

                _enemySpawnManager.AddEnemyType(scene);
                GD.Print($"[GameDirector] Unlocked enemy type: {unlock.ScenePath} at t={_elapsed:F1}s");
            }
        }
    }

    private void OnElementPicked(Element picked, bool isSecondPick)
    {
        if (_runState == null || KitDatabase == null || _spellController == null)
            return;

        var finalElement = _runState.GetCombined();
        if (finalElement == Element.None)
            return;

        var kit = KitDatabase.GetKit(finalElement);
        if (kit == null)
        {
            GD.PrintErr($"No kit found for final element {finalElement}");
            return;
        }

        _spellController.EquipKit(kit);
    }

    private void EndMatch()
    {
        _isRunning = false;

        // Zatrzymanie nowych spawnów
        _enemySpawnManager.StopSpawning();

        GD.Print("[GameDirector] Match ended.");

        // TODO:
        // - odpal ekran podsumowania
        // - zapis progresu
        // - emisja sygnału przez GameEvents itp.
        // GameEvents.Instance.EmitRunEnded();
    }

    // ================== POMOCNICZA KLASA DO UNLOCKÓW ==================

    private class EnemyUnlockConfig
    {
        public float UnlockTimeSeconds;
        public string ScenePath;
        public bool Unlocked;

        public EnemyUnlockConfig(float unlockTimeSeconds, string scenePath)
        {
            UnlockTimeSeconds = unlockTimeSeconds;
            ScenePath = scenePath;
            Unlocked = false;
        }
    }
}
