using Godot;

public partial class GameEvents : Node
{
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void RunEndedEventHandler();

    [Signal] public delegate void ExperiencePointCollectedEventHandler(int currentExperience);
    [Signal] public delegate void RunTimeUpdatedEventHandler(float elapsed, float total);

    [Signal] public delegate void MenuEnabledEventHandler(bool isMenuEnabled);

    // --- CHEST REWARD FLOW ---
    // Request = "Hej UI, pokaż overlay skrzynki z tym itemem"
    [Signal] public delegate void ChestRewardRequestedEventHandler(ItemDefinition item, int cost);

    // Optional: UI może chcieć wiedzieć, że overlay wystartował / zakończył się (np. do input/mouse/pause)
    [Signal] public delegate void ChestRewardStartedEventHandler();
    [Signal] public delegate void ChestRewardEndedEventHandler();

    // Claim = "gracz zaakceptował nagrodę"
    [Signal] public delegate void ChestRewardClaimedEventHandler(ItemDefinition item);

    // Closed = "overlay zamknięty, wracamy do gry"
    [Signal] public delegate void ChestRewardClosedEventHandler();

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

    public void EmitExperiencePointCollected(int currentExperience)
        => EmitSignal(nameof(ExperiencePointCollected), currentExperience);

    public void EmitRunTimeUpdated(float elapsed, float total)
        => EmitSignal(nameof(RunTimeUpdated), elapsed, total);

    public void EmitMenuEnabled(bool isMenuEnabled)
        => EmitSignal(nameof(MenuEnabled), isMenuEnabled);

    // --- Chest API (to czego ChestRewardOverlay oczekuje + to czego UI potrzebuje) ---
    public void RequestChestReward(ItemDefinition item, int chestOpenCost)
        => EmitSignal(nameof(ChestRewardRequested), item, chestOpenCost);

    public void EmitChestRewardStarted()
        => EmitSignal(nameof(ChestRewardStarted));

    public void EmitChestRewardEnded()
        => EmitSignal(nameof(ChestRewardEnded));

    public void ClaimChestReward(ItemDefinition item)
        => EmitSignal(nameof(ChestRewardClaimed), item);

    public void CloseChestReward()
        => EmitSignal(nameof(ChestRewardClosed));

    // --- Element pick ---
    public void RequestElementPick(bool isSecondPick)
        => EmitSignal(nameof(ElementPickRequested), isSecondPick);

    public void EmitElementPicked(Element picked, bool isSecondPick)
        => EmitSignal(nameof(ElementPicked), (int)picked, isSecondPick);
}
