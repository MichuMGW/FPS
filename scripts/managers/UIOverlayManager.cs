using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UIOverlayManager : Node
{
    [Export] public PackedScene ElementOverlayScene;
    [Export] public PackedScene ChestRewardOverlayScene;
    [Export] public PackedScene LevelUpOverlayScene;
    [Export] public PackedScene GameMenuScene;

    // ✅ NOWE: End game overlay
    [Export] public PackedScene EndGameOverlayScene;

    [Export] public ElementKitDatabase KitDatabase;

    private GameEvents _events;
    private PauseManager _pause;

    private bool _overlayOpen = false;
    private readonly Queue<Godot.Collections.Array<LevelUpOffer>> _levelUpQueue = new();
    private CanvasLayer _activeOverlay;

    // ✅ NOWE: żeby nie odpalić end overlay 2 razy
    private bool _endGameScheduled = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _overlayOpen = false;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        EnsurePause();

        if (_events == null)
            GD.PushError("[UIOverlayManager] Missing GameEvents autoload.");
        if (ElementOverlayScene == null)
            GD.PushError("[UIOverlayManager] ElementOverlayScene not assigned.");
        if (KitDatabase == null)
            GD.PushWarning("[UIOverlayManager] KitDatabase not assigned (you can still assign it in ElementOverlay scene).");

        if (_events != null)
        {
            _events.ElementPickRequested += OnElementPickRequested;
            _events.ChestRewardRequested += OnChestRewardRequested;
            _events.GameMenuRequested += OnGameMenuRequested;
            _events.LevelUpChoiceRequested += OnLevelUpChoiceRequested;

            _events.BossEnded += OnBossEnded;
            _events.RunEnded += OnRunEnded;
        }
    }

    public override void _ExitTree()
    {
        if (_events != null)
        {
            _events.ElementPickRequested -= OnElementPickRequested;
            _events.ChestRewardRequested -= OnChestRewardRequested;
            _events.GameMenuRequested -= OnGameMenuRequested;
            _events.LevelUpChoiceRequested -= OnLevelUpChoiceRequested;

            _events.BossEnded -= OnBossEnded;
            _events.RunEnded -= OnRunEnded;
        }
    }

    private void EnsurePause()
    {
        // Uwaga: przy ChangeScene CurrentScene bywa null przez moment
        if (_pause != null && GodotObject.IsInstanceValid(_pause) && _pause.IsInsideTree())
            return;

        var tree = GetTree();
        if (tree == null) return;

        _pause = tree.CurrentScene?.GetNodeOrNull<PauseManager>("PauseManager");
    }

    private bool PauseIsValid()
        => _pause != null && GodotObject.IsInstanceValid(_pause) && _pause.IsInsideTree();

    // =========================
    // ✅ END GAME FLOW (BOSS)
    // =========================

    private async void OnBossEnded(Node boss)
    {
        if (_endGameScheduled) return;
        _endGameScheduled = true;

        // Poczekaj 3 sekundy po “boss ended”
        var tree = GetTree();
        if (tree == null) return;

        // Timer ma działać nawet gdybyś gdzieś miał pause / timescale
        var t = tree.CreateTimer(10.0, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
        await ToSignal(t, SceneTreeTimer.SignalName.Timeout);

        // Jeśli w międzyczasie node wyleciał z drzewa (zmiana sceny), to nie rób nic
        if (!IsInsideTree()) return;

        ShowEndGameOverlayForce();
    }

    private async void OnRunEnded(int reason)
    {
        if (_endGameScheduled) return;
        _endGameScheduled = true;

        var tree = GetTree();
        if (tree == null) return;

        if (!IsInsideTree()) return;

        ShowEndGameOverlayForce();
    }


    private void ShowEndGameOverlayForce()
    {
        if (EndGameOverlayScene == null)
        {
            GD.PushWarning("[UIOverlayManager] EndGameOverlayScene not assigned.");
            return;
        }

        // Wyczyść kolejki i zamknij aktywny overlay bez zabawy w “czekaj aż łaskawie zniknie”
        _levelUpQueue.Clear();
        ForceCloseActiveOverlay();

        var overlay = EndGameOverlayScene.Instantiate() as CanvasLayer;
        if (overlay == null)
        {
            GD.PushError("[UIOverlayManager] EndGameOverlayScene is not a CanvasLayer.");
            return;
        }

        overlay.ProcessMode = ProcessModeEnum.Always;

        ShowOverlay(overlay);
        overlay.TreeExited += OnOverlayTreeExited;
    }

    private void ForceCloseActiveOverlay()
    {
        // Jeśli coś było otwarte (menu/levelup/chest), to ubijamy to bez odpalania OnOverlayTreeExited,
        // bo tam byś próbował dotykać PauseManagera w trakcie ChangeScene i lecą ObjectDisposedException.
        if (_activeOverlay != null && GodotObject.IsInstanceValid(_activeOverlay))
        {
            _activeOverlay.TreeExited -= OnOverlayTreeExited;
            _activeOverlay.QueueFree();
        }

        _activeOverlay = null;
        _overlayOpen = false;

        EnsurePause();
        if (PauseIsValid())
        {
            // Spróbuj zdjąć pauzę, jeśli coś zostało po poprzednim overlayu
            _pause.ReleasePause();
        }
    }

    // =========================
    // Twoje istniejące overlaye
    // =========================

    private void OnElementPickRequested(bool isSecondPick)
    {
        if (_overlayOpen)
        {
            GD.PushWarning("[UIOverlayManager] ElementPickRequested while overlay open. Ignored.");
            return;
        }

        if (ElementOverlayScene == null)
            return;

        var overlay = ElementOverlayScene.Instantiate() as ElementOverlay;
        if (overlay == null)
        {
            GD.PushError("[UIOverlayManager] ElementOverlayScene is not an ElementOverlay.");
            return;
        }

        overlay.ProcessMode = ProcessModeEnum.Always;
        overlay.IsSecondPick = isSecondPick;

        if (overlay.KitDatabase == null)
            overlay.KitDatabase = KitDatabase;

        ShowOverlay(overlay);
        overlay.TreeExited += OnOverlayTreeExited;
    }

    private void OnChestRewardRequested(ChestRewardContext ctx)
    {
        if (_overlayOpen)
        {
            GD.PushWarning("[UIOverlayManager] ChestRewardRequested while overlay open. Ignored.");
            return;
        }

        if (ChestRewardOverlayScene == null || _events == null || ctx == null || ctx.Item == null)
        {
            GD.PushWarning("[UIOverlayManager] Cannot start ChestRewardOverlay: missing scene/events/ctx/item.");
            return;
        }

        var overlay = ChestRewardOverlayScene.Instantiate() as ChestRewardOverlay;
        if (overlay == null)
        {
            GD.PushError("[UIOverlayManager] ChestRewardOverlayScene is not a ChestRewardOverlay.");
            return;
        }

        overlay.ProcessMode = ProcessModeEnum.Always;

        ShowOverlay(overlay);
        overlay.Start(ctx, _events);

        overlay.TreeExited += OnOverlayTreeExited;
    }

    private void OnLevelUpChoiceRequested(Godot.Collections.Array<LevelUpOffer> options)
    {
        if (LevelUpOverlayScene == null || _events == null)
        {
            GD.PushWarning("[UIOverlayManager] Cannot start LevelUpOverlay: missing scene/events.");
            return;
        }

        if (options == null || options.Count == 0)
        {
            GD.PushWarning("[UIOverlayManager] LevelUpChoiceRequested called with empty options.");
            return;
        }

        _levelUpQueue.Enqueue(options);
        TryShowNextLevelUp();
    }

    private void TryShowNextLevelUp()
    {
        if (_overlayOpen) return;
        if (_levelUpQueue.Count <= 0) return;

        var next = _levelUpQueue.Dequeue();
        ShowLevelUpOverlay(next);
    }

    private void ShowLevelUpOverlay(Godot.Collections.Array<LevelUpOffer> options)
    {
        var overlay = LevelUpOverlayScene.Instantiate() as LevelUpOverlay;
        if (overlay == null)
        {
            GD.PushError("[UIOverlayManager] LevelUpOverlayScene is not a LevelUpOverlay.");
            return;
        }

        overlay.ProcessMode = ProcessModeEnum.Always;
        ShowOverlay(overlay);

        overlay.SetOptions(options);

        overlay.Picked += (LevelUpOffer picked) =>
        {
            _events.ResolveLevelUpChoice(picked);
            overlay.QueueFree();
        };

        overlay.TreeExited += OnOverlayTreeExited;
    }

    private void OnGameMenuRequested()
    {
        if (_overlayOpen)
        {
            GD.PushWarning("[UIOverlayManager] GameMenuRequested while overlay open. Ignored.");
            return;
        }

        if (GameMenuScene == null)
            return;

        var menu = GameMenuScene.Instantiate() as GameMenuUI;
        if (menu == null)
        {
            GD.PushError("[UIOverlayManager] GameMenuScene is not a GameMenuUI.");
            return;
        }

        menu.ProcessMode = ProcessModeEnum.Always;
        ShowOverlay(menu);
        menu.TreeExited += OnOverlayTreeExited;
    }

    // =========================
    // Common open/close
    // =========================

    private void ShowOverlay(CanvasLayer overlay)
    {
        _overlayOpen = true;
        _activeOverlay = overlay;

        EnsurePause();
        if (PauseIsValid())
            _pause.RequestPause();

        var scene = GetTree().CurrentScene;
        if (scene != null)
            scene.AddChild(overlay);
        else
            GetTree().Root.AddChild(overlay);

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnOverlayTreeExited()
    {
        _activeOverlay = null;
        _overlayOpen = false;

        EnsurePause();
        if (PauseIsValid())
            _pause.ReleasePause();

        bool hasNextLevelUp = _levelUpQueue.Count > 0;
        Input.MouseMode = hasNextLevelUp ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

        TryShowNextLevelUp();
    }
}
