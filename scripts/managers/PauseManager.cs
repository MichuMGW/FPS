using Godot;
using System;

public partial class PauseManager : Node
{
    private int _pauseRequests = 0;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public void RequestPause()
    {
        _pauseRequests++;
        GetTree().Paused = true;
    }

    public void ReleasePause()
    {
        _pauseRequests = Math.Max(0, _pauseRequests - 1);
        if (_pauseRequests == 0)
            GetTree().Paused = false;
    }

    public bool IsPausedByRequests => _pauseRequests > 0;
}
