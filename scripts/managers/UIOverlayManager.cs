using Godot;

public partial class UIOverlayManager : Node
{
    [Export] public PackedScene ElementOverlayScene;
    [Export] public PackedScene ChestRewardOverlayScene; // <-- DODAJ
    [Export] public ElementKitDatabase KitDatabase;

    private GameEvents _events;
    private PauseManager _pause;

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
            _events.ChestRewardRequested += OnChestRewardRequested; // <-- DODAJ
        }
    }

    private void OnElementPickRequested(bool isSecondPick)
    {
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

        _pause?.RequestPause();

        GetTree().Root.AddChild(overlay);
        Input.MouseMode = Input.MouseModeEnum.Visible;

        overlay.TreeExited += () =>
        {
            _pause?.ReleasePause();
            Input.MouseMode = Input.MouseModeEnum.Captured;
        };
    }

    private void OnChestRewardRequested(ItemDefinition item, int cost)
{
    if (ChestRewardOverlayScene == null || _events == null || item == null)
        {
            GD.PushWarning("[UIOverlayManager] Cannot start ChestRewardOverlay: missing scene, events, or item.");
            return;
        }
        

    var overlay = ChestRewardOverlayScene.Instantiate() as ChestRewardOverlay;
    if (overlay == null)
    {
        GD.PushError("[UIOverlayManager] ChestRewardOverlayScene is not a ChestRewardOverlay.");
        return;
    }

    overlay.ProcessMode = ProcessModeEnum.Always;

    _pause?.RequestPause();
    GetTree().Root.AddChild(overlay);
    Input.MouseMode = Input.MouseModeEnum.Visible;

    // Start overlay: UI dane bierze z ItemDefinition
    overlay.Start(item, _events);

    overlay.TreeExited += () =>
    {
        _pause?.ReleasePause();
        Input.MouseMode = Input.MouseModeEnum.Captured;
    };
}

}
