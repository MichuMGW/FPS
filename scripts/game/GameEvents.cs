using Godot;

public partial class GameEvents : Node
{
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void RunEndedEventHandler();

    [Signal] public delegate void GameMenuRequestedEventHandler();

    [Signal] public delegate void RunTimeUpdatedEventHandler(float elapsed, float total);

    [Signal] public delegate void MenuEnabledEventHandler(bool isMenuEnabled);

    // --- CHEST REWARD FLOW ---
    [Signal] public delegate void ChestRewardRequestedEventHandler(ChestRewardContext ctx);
    [Signal] public delegate void ChestRewardResolvedEventHandler(ChestRewardContext ctx, bool claimed);

    // Element pick
    [Signal] public delegate void ElementPickRequestedEventHandler(bool isSecondPick);
    [Signal] public delegate void ElementPickedEventHandler(int pickedElement, bool isSecondPick);

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    // ===== Emittery / Requests =====
    public void StartGame() => EmitSignal(nameof(GameStarted));
    public void EndRun() => EmitSignal(nameof(RunEnded));
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
}
