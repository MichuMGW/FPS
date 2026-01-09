using Godot;
using System;

public partial class PauseManager : Node
{
    private GameEvents _events;
    private int _pauseRequests = 0;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
    }

    public override void _Input(InputEvent e)
    {
        if (!e.IsActionPressed("ui_cancel"))
            return;

        // Jeśli jest otwarty inny overlay, to on ma zjeść ESC (np. ChestRewardOverlay)
        // Zmienisz to jak będziesz chciał inne zachowanie.
        if (HasBlockingOverlayOpen())
            return;

        _events?.RequestGameMenu();
        GetViewport().SetInputAsHandled();
    }

    private bool HasBlockingOverlayOpen()
    {
        return GetTree().GetNodesInGroup("overlay").Count > 0;
    }

    public void RequestPause()
    {
        _pauseRequests++;
        if (!IsInsideTree()) return;
        GetTree().Paused = true;
    }

    public void ReleasePause()
    {
        _pauseRequests = Math.Max(0, _pauseRequests - 1);
        if (!IsInsideTree()) return;

        if (_pauseRequests == 0)
            GetTree().Paused = false;
    }

    public bool IsPausedByRequests => _pauseRequests > 0;
}
