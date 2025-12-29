using Godot;
using System.Collections.Generic;

public partial class GameDirector : Node
{
    [Export] public float MatchDurationSeconds { get; set; } = 600f; // 10 minut
    [Export] public NodePath EnemySpawnManagerPath { get; set; }

    [Export] public ElementKitDatabase KitDatabase;

    private EnemySpawnManager _enemySpawnManager;
    private readonly DifficultyManager _difficultyManager = new DifficultyManager();

    private GameEvents _events;
    private RunElementState _runState;
    private PlayerSpellController _spellController;
    private ItemInventory _inventory = null;
    private ChestManager _chestManager = null;

    private float _elapsed;
    private float _spawnTimer;
    private bool _isRunning;

    private readonly List<EnemyUnlockConfig> _enemyUnlocks = new()
    {
        new EnemyUnlockConfig(   0f, "res://scenes/entities/enemies/Skeleton.tscn"),
        new EnemyUnlockConfig(  0f, "res://scenes/entities/enemies/TrollArcher.tscn"),
        new EnemyUnlockConfig( 120f, "res://scenes/entities/enemies/Orc.tscn"),
        new EnemyUnlockConfig( 180f, "res://scenes/entities/enemies/SkeletonSummoner.tscn"),
    };

    public override void _Ready()
    {
        if (EnemySpawnManagerPath == null || EnemySpawnManagerPath.IsEmpty)
        {
            GD.PushError("[GameDirector] EnemySpawnManagerPath is not set.");
            return;
        }

        _enemySpawnManager = GetNodeOrNull<EnemySpawnManager>(EnemySpawnManagerPath);
        if (_enemySpawnManager == null)
        {
            GD.PushError("[GameDirector] Could not find EnemySpawnManager.");
            return;
        }

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _runState = GetTree().Root.GetNodeOrNull<RunElementState>("RunElementState");
        _inventory = GetTree().Root.GetNodeOrNull<ItemInventory>("ItemInventory");
        // _chestManager = GetTree().Root.GetNodeOrNull<ChestManager>("ChestManager");
        _chestManager = GetTree().GetFirstNodeInGroup("chest_manager") as ChestManager;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        _spellController = player.GetNodeOrNull<PlayerSpellController>("PlayerSpellController");

        if (_events == null)
            GD.PushError("[GameDirector] Missing GameEvents autoload.");
        if (_runState == null)
            GD.PushError("[GameDirector] Missing RunElementState autoload.");
        if (_spellController == null)
            GD.PushWarning("[GameDirector] PlayerSpellController not found (kit equip won't work).");
        if (KitDatabase == null)
            GD.PushWarning("[GameDirector] KitDatabase not assigned.");

        if (_events != null)
        {
            _events.ElementPicked += OnElementPicked;
        }

        CallDeferred(nameof(BootstrapRun));
        // Na start zawsze wybór pierwszego żywiołu
        
    }

    private async void BootstrapRun()
    {
        // Poczekaj aż drzewo zakończy setup (1 frame wystarcza w 99% przypadków)
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Dodatkowo: upewnij się, że UIOverlayManager i inne autoloady weszły
        // (opcjonalnie, ale stabilne)
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Teraz dopiero start:
        StartGame();
    }

    public override void _Process(double delta)
    {
        if (!_isRunning)
            return;

        _elapsed += (float)delta;

        _events?.EmitRunTimeUpdated(_elapsed, MatchDurationSeconds);

        if (_elapsed >= MatchDurationSeconds)
        {
            EndMatch();
            return;
        }

        // Pobierz snapshot trudności dla aktualnego czasu
        // TODO: DODAĆ POZIOMY TRUDNOŚCI DO WYBORU
        DifficultySnapshot diff = _difficultyManager.GetDifficulty(_elapsed, RunDifficulty.Normal);

        _enemySpawnManager.UpdateDifficulty(diff);

        UnlockEnemiesIfNeeded();

        if (_enemySpawnManager.AliveEnemiesCount >= diff.MaxAliveEnemies)
            return;

        _spawnTimer -= (float)delta;
        if (_spawnTimer <= 0f)
        {
            _enemySpawnManager.SpawnRandomEnemy();
            _spawnTimer = diff.SpawnInterval;
        }
    }

    private void StartGame()
    {
        GD.Print("[GameDirector] Match started.");
        
        _isRunning = true;
        _elapsed = 0f;
        _spawnTimer = 0f;
        _enemySpawnManager.StartSpawning();
        _chestManager.SpawnChests();

        _events.RequestElementPick(isSecondPick: false);
    }

    private void UnlockEnemiesIfNeeded()
    {
        foreach (var unlock in _enemyUnlocks)
        {
            if (unlock.Unlocked || _elapsed < unlock.UnlockTimeSeconds)
                continue;

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

    // ===== Element pick handling =====

    private void OnElementPicked(int pickedElement, bool isSecondPick)
    {
        if (_runState == null || _events == null)
            return;

        var picked = (Element)pickedElement;

        if (!isSecondPick)
        {
            _runState.SetFirst(picked);

            // Po pierwszym wyborze możesz od razu założyć bazowy kit,
            // albo czekać aż gracz wybierze drugi element w trakcie runa.
            ApplyKitFromRunState();

            return;
        }

        _runState.SetSecond(picked);
        ApplyKitFromRunState();
    }

    private void ApplyKitFromRunState()
    {
        if (_runState == null || KitDatabase == null || _spellController == null)
            return;

        var finalElement = _runState.GetCombined();
        if (finalElement == Element.None)
            return;

        var kit = KitDatabase.GetKit(finalElement);
        if (kit == null)
        {
            GD.PrintErr($"[GameDirector] No kit found for final element {finalElement}");
            return;
        }

        _spellController.EquipKit(kit);
        GD.Print($"[GameDirector] Equipped kit: {finalElement}");
    }

    // ===== Example hook: call this when you want second pick during run =====
    public void RequestSecondElementPick()
    {
        if (_runState == null || _events == null)
            return;

        if (!_runState.HasFirst)
        {
            _events.RequestElementPick(isSecondPick: false);
            return;
        }

        if (_runState.HasSecond)
            return;

        _events.RequestElementPick(isSecondPick: true);
    }

    private void EndMatch()
    {
        _isRunning = false;
        _enemySpawnManager.StopSpawning();

        GD.Print("[GameDirector] Match ended.");

        _events?.EndRun();
    }

    // ================== helper ==================

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
