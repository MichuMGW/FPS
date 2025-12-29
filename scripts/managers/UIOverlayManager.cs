using Godot;
using System.Collections.Generic;

public partial class UIOverlayManager : Node
{
    [Export] public PackedScene ElementOverlayScene;
    [Export] public PackedScene ChestRewardOverlayScene;
    [Export] public PackedScene LevelUpOverlayScene;
    [Export] public PackedScene GameMenuScene;
    [Export] public ElementKitDatabase KitDatabase;

    private GameEvents _events;
    private PauseManager _pause;

    // ==== Overlay queue (na raz pokazujemy jeden) ====
    private bool _overlayOpen = false;

    // Level-upy potrafią przyjść “hurtem”, więc je kolejkujemy
    private readonly Queue<Godot.Collections.Array<LevelUpOffer>> _levelUpQueue = new();

    // (Opcjonalnie) trzymaj referencję do aktualnego overlay, jak chcesz blokować inne requesty
    private CanvasLayer _activeOverlay;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _pause = GetTree().Root.GetNodeOrNull<PauseManager>("PauseManager");

        if (_events == null)
            GD.PushError("[UIOverlayManager] Missing GameEvents autoload.");
        if (_pause == null)
            GD.PushError("[UIOverlayManager] Missing PauseManager autoload.");
        if (ElementOverlayScene == null)
            GD.PushError("[UIOverlayManager] ElementOverlayScene not assigned.");
        if (ChestRewardOverlayScene == null)
            GD.PushWarning("[UIOverlayManager] ChestRewardOverlayScene not assigned.");
        if (KitDatabase == null)
            GD.PushWarning("[UIOverlayManager] KitDatabase not assigned (you can still assign it in ElementOverlay scene).");

        if (_events != null)
        {
            _events.ElementPickRequested += OnElementPickRequested;
            _events.ChestRewardRequested += OnChestRewardRequested;
            _events.GameMenuRequested += OnGameMenuRequested;
            _events.LevelUpChoiceRequested += OnLevelUpChoiceRequested;
        }
    }

    private void OnElementPickRequested(bool isSecondPick)
    {
        // Jeśli coś już jest otwarte, ignoruj albo kolejkuj (Twoja decyzja).
        // Tu: ignorujemy, bo element pick raczej nie powinien wpadać w trakcie innych overlayów.
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
        // chesty też możesz kolejować, ale na razie blokujemy jak coś jest otwarte
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

        // Zawsze kolejkuj level-upy
        _levelUpQueue.Enqueue(options);

        // Jeśli nic nie jest otwarte, pokaż od razu
        TryShowNextLevelUp();
    }

    private void TryShowNextLevelUp()
    {
        if (_overlayOpen)
            return;

        if (_levelUpQueue.Count <= 0)
            return;

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

    // ===== Common overlay open/close =====

    private void ShowOverlay(CanvasLayer overlay)
    {
        _overlayOpen = true;
        _activeOverlay = overlay;

        _pause?.RequestPause();
        GetTree().Root.AddChild(overlay);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnOverlayTreeExited()
    {
        _activeOverlay = null;
        _overlayOpen = false;

        _pause?.ReleasePause();

        bool hasNextLevelUp = _levelUpQueue.Count > 0;

        if (!hasNextLevelUp)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        TryShowNextLevelUp();
    }
}
