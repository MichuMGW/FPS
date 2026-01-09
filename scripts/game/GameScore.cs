using Godot;
using System;

public partial class GameScore : Node
{
    public sealed class Result
    {
        public RunEndReason Reason;
        public string Difficulty;
        public int EnemiesKilled;
        public int GoldCollected;
        public int ItemsCollected;
        public int PlayerLevel;
        public float RunTimeSeconds;
    }

    private string _difficulty = "";
    private int _enemyKilled = 0;
    private int _goldCollected = 0;
    private int _itemsCollected = 0;
    private int _playerLevel = 1;

    private float _normalElapsed = 0f;
    private float _normalTotal = 0f;
    private float _bossElapsed = 0f;
    private float _lastRunTotal = 0f;

    private GameEvents _events;
    private RunConfig _runConfig;
    private GoldManager _gold;
    private ExperienceManager _experience;
    private ItemInventory _inventory;

    private bool _frozen = false;
    public Result LastResult { get; private set; }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _runConfig = GetTree().Root.GetNodeOrNull<RunConfig>("RunConfig");

        _gold = GetTree().CurrentScene?.GetNodeOrNull<GoldManager>("GoldManager");
        _experience = GetTree().CurrentScene?.GetNodeOrNull<ExperienceManager>("ExperienceManager");
        _inventory = GetTree().CurrentScene?.GetNodeOrNull<ItemInventory>("ItemInventory");

        if (_events != null)
        {
            _events.EnemyDied += OnEnemyDied;
            _events.RunEnded += OnRunEnded;               // <- TERAZ Z ARGUMENTEM
            _events.RunTimeUpdated += OnRunTimeUpdated;
            _events.GameStarted += ResetRun;              // <- polecam, żeby nowy run był czysty
        }

        ResetRun();
    }

    public override void _ExitTree()
    {
        if (_events != null)
        {
            _events.EnemyDied -= OnEnemyDied;
            _events.RunEnded -= OnRunEnded;
            _events.RunTimeUpdated -= OnRunTimeUpdated;
            _events.GameStarted -= ResetRun;
        }
    }

    public void ResetRun()
    {
        _frozen = false;
        LastResult = null;

        _enemyKilled = 0;
        _goldCollected = 0;
        _itemsCollected = 0;
        _playerLevel = 1;

        _difficulty = _runConfig?.GetDifficultyName() ?? "";

        _normalElapsed = 0f;
        _normalTotal = 0f;
        _bossElapsed = 0f;
    }

    private void OnEnemyDied()
    {
        if (_frozen) return;
        _enemyKilled++;
    }

    private void OnRunTimeUpdated(float elapsed, float total)
    {
        if (_frozen) return;

        if (total > 0.0001f)
        {
            _normalElapsed = elapsed;
            _normalTotal = total;
            return;
        }

        // boss phase: total == 0
        _bossElapsed = elapsed;
    }

    private void OnRunEnded(int reasonInt)
    {
        if (_frozen) return;

        var reason = (RunEndReason)reasonInt;
        FreezeScore(reason);
    }

    public void FreezeScore(RunEndReason reason)
    {
        if (_frozen) return;
        _frozen = true;

        _difficulty = _runConfig?.GetDifficultyName() ?? _difficulty;

        _goldCollected = _gold != null ? _gold.GoldGained : _goldCollected;
        _playerLevel = _experience != null ? _experience.CurrentLevel : _playerLevel;

        _itemsCollected = 0;
        if (_inventory != null)
        {
            foreach (var (_, count) in _inventory.GetStacks())
                _itemsCollected += Math.Max(0, count);
        }

        float runTimeSeconds;

        if (reason == RunEndReason.RunFinished)
        {
            // ukończone: baza + boss
            runTimeSeconds = Mathf.Max(0f, _normalTotal) + Mathf.Max(0f, _bossElapsed);
        }
        else
        {
            // śmierć: NIE dodajemy match duration
            // jeśli boss się zaczął, pokazuj tylko czas bossa, inaczej normal elapsed
            runTimeSeconds = (_bossElapsed > 0.0001f) ? Mathf.Max(0f, _bossElapsed) : Mathf.Max(0f, _normalElapsed);
        }

        LastResult = new Result
        {
            Reason = reason,
            Difficulty = _difficulty,
            EnemiesKilled = _enemyKilled,
            GoldCollected = _goldCollected,
            ItemsCollected = _itemsCollected,
            PlayerLevel = _playerLevel,
            RunTimeSeconds = runTimeSeconds
        };
    }


    public Result GetLiveSnapshot(RunEndReason reasonGuess = RunEndReason.RunFinished)
    {
        int items = 0;
        if (_inventory != null)
        {
            foreach (var (_, count) in _inventory.GetStacks())
                items += Math.Max(0, count);
        }

        float runTimeSeconds =
            reasonGuess == RunEndReason.RunFinished
                ? Mathf.Max(0f, _normalTotal) + Mathf.Max(0f, _bossElapsed)
                : (_bossElapsed > 0.0001f ? Mathf.Max(0f, _bossElapsed) : Mathf.Max(0f, _normalElapsed));

        return new Result
        {
            Reason = reasonGuess,
            Difficulty = _runConfig?.GetDifficultyName() ?? _difficulty,
            EnemiesKilled = _enemyKilled,
            GoldCollected = _gold != null ? _gold.GoldGained : _goldCollected,
            ItemsCollected = items,
            PlayerLevel = _experience != null ? _experience.CurrentLevel : _playerLevel,
            RunTimeSeconds = runTimeSeconds
        };
    }
}
