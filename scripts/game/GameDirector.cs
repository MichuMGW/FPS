using Godot;
using System.Collections.Generic;

public partial class GameDirector : Node
{
    [Export] public float MatchDurationSeconds { get; set; } = 600f;
    [Export] public NodePath EnemySpawnManagerPath { get; set; }
    [Export] public ElementKitDatabase KitDatabase;
    [Export(PropertyHint.Range, "0.1,0.9,0.05")]
    public float SecondPickAtNormalizedTime = 0.5f;

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

    private bool _secondPickRequested;
    private bool _secondPickDone;

    private readonly List<EnemyUnlockConfig> _enemyUnlocks = new()
    {
        new EnemyUnlockConfig(0f, "res://scenes/entities/enemies/Skeleton.tscn", weight: 3f),
        new EnemyUnlockConfig(60f, "res://scenes/entities/enemies/TrollArcher.tscn", weight: 1.2f),
        new EnemyUnlockConfig(120f, "res://scenes/entities/enemies/Orc.tscn", weight: 1.5f, minEnemyLevel: 3),
        new EnemyUnlockConfig(300f, "res://scenes/entities/enemies/SkeletonSummoner.tscn", weight: 0.5f, minCoeff: 1.05f),
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
        _chestManager = GetTree().GetFirstNodeInGroup("chest_manager") as ChestManager;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        _spellController = player?.GetNodeOrNull<PlayerSpellController>("PlayerSpellController");

        if (_events == null)
            GD.PushError("[GameDirector] Missing GameEvents autoload.");
        if (_runState == null)
            GD.PushError("[GameDirector] Missing RunElementState autoload.");
        if (_spellController == null)
            GD.PushWarning("[GameDirector] PlayerSpellController not found (kit equip won't work).");
        if (KitDatabase == null)
            GD.PushWarning("[GameDirector] KitDatabase not assigned.");

        if (_events != null)
            _events.ElementPicked += OnElementPicked;

        CallDeferred(nameof(BootstrapRun));
    }

    private async void BootstrapRun()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        StartGame();
    }

    public override void _Process(double delta)
    {
        if (!_isRunning)
            return;

        _elapsed += (float)delta;
        _events?.EmitRunTimeUpdated(_elapsed, MatchDurationSeconds);

        TryTriggerSecondPick();

        if (_elapsed >= MatchDurationSeconds)
        {
            EndMatch();
            return;
        }

        DifficultySnapshot diff = _difficultyManager.GetDifficulty(_elapsed, RunDifficulty.Normal);
        _enemySpawnManager.UpdateDifficulty(diff);

        UnlockEnemiesIfNeeded(diff);

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

        _secondPickRequested = false;
        _secondPickDone = false;

        _enemySpawnManager.StartSpawning();
        _chestManager?.SpawnChests();

        _events?.RequestElementPick(isSecondPick: false);
    }

    private void UnlockEnemiesIfNeeded(DifficultySnapshot diff)
    {
        foreach (var unlock in _enemyUnlocks)
        {
            if (unlock.Unlocked || _elapsed < unlock.UnlockTimeSeconds)
                continue;

            // dodatkowe filtry pod difficulty:
            if (diff.EnemyLevel < unlock.MinEnemyLevel || diff.EnemyLevel > unlock.MaxEnemyLevel)
                continue;
            if (diff.Coeff < unlock.MinCoeff || diff.Coeff > unlock.MaxCoeff)
                continue;

            unlock.Unlocked = true;

            var scene = GD.Load<PackedScene>(unlock.ScenePath);
            if (scene == null)
            {
                GD.PushError($"[GameDirector] Cannot load enemy scene at path: {unlock.ScenePath}");
                continue;
            }

            _enemySpawnManager.AddEnemyType(scene, unlock.Weight);
            GD.Print($"[GameDirector] Unlocked enemy type: {unlock.ScenePath} weight={unlock.Weight} at t={_elapsed:F1}s");
        }
    }

    private void TryTriggerSecondPick()
    {
        if (_events == null || _runState == null) return;

        if (_secondPickDone || _secondPickRequested) return;
        if (!_runState.HasFirst || _runState.HasSecond) return;

        //float triggerTime = MatchDurationSeconds * Mathf.Clamp(SecondPickAtNormalizedTime, 0.1f, 0.9f);
        float triggerTime = 1;

        if (_elapsed < triggerTime)
            return;

        _secondPickRequested = true;

        // pauza gry, overlay ma ProcessMode Always więc będzie działał
        GetTree().Paused = true;

        _events.RequestElementPick(isSecondPick: true);
        GD.Print($"[GameDirector] Second element pick requested at t={_elapsed:F1}s");
    }

    private void OnElementPicked(int pickedElement, bool isSecondPick)
    {
        if (_runState == null || _events == null)
            return;

        var picked = (Element)pickedElement;

        if (!isSecondPick)
        {
            _runState.SetFirst(picked);
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
}
