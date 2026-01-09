using Godot;

public partial class GameEvents : Node
{
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void RunEndedEventHandler(int reason);

    [Signal] public delegate void GameMenuRequestedEventHandler();

    [Signal] public delegate void RunTimeUpdatedEventHandler(float elapsed, float total);

    [Signal] public delegate void MenuEnabledEventHandler(bool isMenuEnabled);

    // --- CHEST REWARD FLOW ---
    [Signal] public delegate void ChestRewardRequestedEventHandler(ChestRewardContext ctx);
    [Signal] public delegate void ChestRewardResolvedEventHandler(ChestRewardContext ctx, bool claimed);

    // Element pick
    [Signal] public delegate void ElementPickRequestedEventHandler(bool isSecondPick);
    [Signal] public delegate void ElementPickedEventHandler(int pickedElement, bool isSecondPick);
    // Level up
    [Signal] public delegate void LevelUpChoiceRequestedEventHandler(Godot.Collections.Array<LevelUpOffer> options);
    [Signal] public delegate void LevelUpChoiceResolvedEventHandler(LevelUpOffer picked);

    [Signal] public delegate void ShowInfoEventHandler(string message, float durationSeconds);
    [Signal] public delegate void HideInfoEventHandler();
    [Signal] public delegate void ShowErrorEventHandler(string message, float durationSeconds);

    [Signal] public delegate void BossFightStartedEventHandler();
    [Signal] public delegate void BossFightEndedEventHandler();

    [Signal] public delegate void BossSpawnedEventHandler(Node boss, BossHealthComponent bossHealth, string displayName);
    [Signal] public delegate void BossEndedEventHandler(Node boss);

    [Signal] public delegate void EnemyDiedEventHandler();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    // ===== Emittery / Requests =====
    public void StartGame() => EmitSignal(nameof(GameStarted));
    public void EndRun(RunEndReason reason = RunEndReason.RunFinished)
        => EmitSignal(nameof(RunEnded), (int)reason);
    public void RequestGameMenu()
        => EmitSignal(nameof(GameMenuRequested));

    public void EmitRunTimeUpdated(float elapsed, float total)
        => EmitSignal(nameof(RunTimeUpdated), elapsed, total);

    public void EmitMenuEnabled(bool isMenuEnabled)
        => EmitSignal(nameof(MenuEnabled), isMenuEnabled);


    // --- Chest API (to czego ChestRewardOverlay oczekuje + to czego UI potrzebuje) ---
    public void RequestChestReward(ItemDefinition item, int chestOpenCost)
        => EmitSignal(nameof(ChestRewardRequested), item, chestOpenCost);

    public void RequestChestReward(ChestRewardContext ctx)
        => EmitSignal(nameof(ChestRewardRequested), ctx);

    public void ResolveChestReward(ChestRewardContext ctx, bool claimed)
        => EmitSignal(nameof(ChestRewardResolved), ctx, claimed);

    // --- Element pick ---
    public void RequestElementPick(bool isSecondPick)
        => EmitSignal(nameof(ElementPickRequested), isSecondPick);

    public void EmitElementPicked(Element picked, bool isSecondPick)
        => EmitSignal(nameof(ElementPicked), (int)picked, isSecondPick);
    
    // --- Level up ---
    public void RequestLevelUpChoice(Godot.Collections.Array<LevelUpOffer> options)
    => EmitSignal(nameof(LevelUpChoiceRequested), options);

    public void ResolveLevelUpChoice(LevelUpOffer picked)
        => EmitSignal(nameof(LevelUpChoiceResolved), picked);

    public void EmitShowInfo(string message, float durationSeconds = 0f)
        => EmitSignal(nameof(ShowInfo), message, durationSeconds);

    public void EmitHideInfo()
        => EmitSignal(nameof(HideInfo));

    public void EmitShowError(string message, float durationSeconds = 3f)
        => EmitSignal(nameof(ShowError), message, durationSeconds);

    public void EmitBossFightStarted()
        => EmitSignal(nameof(BossFightStarted));
    public void EmitBossFightEnded()
        => EmitSignal(nameof(BossFightEnded));

    public void EmitBossSpawned(Node boss, BossHealthComponent bossHealth, string displayName)
    => EmitSignal(nameof(BossSpawned), boss, bossHealth, displayName);

    public void EmitBossEnded(Node boss)
        => EmitSignal(nameof(BossEnded), boss);

    public void EmitEnemyDied()
        => EmitSignal(nameof(EnemyDied));

}
