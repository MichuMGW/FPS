using Godot;
using System;

public partial class LoadingManager : Node
{
    [Signal] public delegate void LoadFailedEventHandler(string message);

    // Jeśli autoload jako scena: możesz to zostawić jako Export.
    // Jeśli autoload jako sam skrypt: wczytaj PackedScene z GD.Load w _Ready i usuń Export.
    [Export] public PackedScene DefaultLoadingOverlay;

    [Export] public float TimeoutSeconds = 120f;       // twardy max (bez znaczenia czy progress rośnie)
    [Export] public float StallTimeoutSeconds = 30f;   // fail jeśli progress stoi w miejscu

    private RunConfig _runConfig;

    private string _path;
    private bool _loading;

    private LoadingOverlay _overlay;
    private Godot.Collections.Array _progress = new();

    private ulong _startMs;
    private ulong _lastProgressMs;
    private float _lastProgress = -1f;

    private double _logAccum;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _runConfig = GetTree().Root.GetNodeOrNull<RunConfig>("RunConfig");
        SetProcess(false);
    }

    public void LoadScene(string path, RunDifficulty? difficulty = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            EmitSignal(SignalName.LoadFailed, "LoadScene: empty path.");
            return;
        }

        if (_loading)
        {
            EmitSignal(SignalName.LoadFailed, "LoadScene: already loading.");
            return;
        }

        if (!ResourceLoader.Exists(path))
        {
            EmitSignal(SignalName.LoadFailed, $"LoadScene: resource does not exist: {path}");
            return;
        }

        if (difficulty.HasValue && _runConfig != null)
            _runConfig.SetDifficulty(difficulty.Value);

        _path = path;
        _loading = true;

        _startMs = Time.GetTicksMsec();
        _lastProgressMs = _startMs;
        _lastProgress = -1f;
        _logAccum = 0;

        // Overlay
        if (DefaultLoadingOverlay != null)
        {
            _overlay = DefaultLoadingOverlay.Instantiate<LoadingOverlay>();
            _overlay.ProcessMode = ProcessModeEnum.Always;
            GetTree().Root.AddChild(_overlay);
            _overlay.SetProgress(0f);
        }

        _progress.Clear();

        // typeHint serio pomaga
        var err = ResourceLoader.LoadThreadedRequest(
            _path,
            typeHint: "PackedScene",
            useSubThreads: true,
            cacheMode: ResourceLoader.CacheMode.Reuse
        );

        if (err != Error.Ok)
        {
            Fail($"LoadThreadedRequest failed: {err} for {_path}");
            return;
        }

        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (!_loading) return;

        _logAccum += delta;

        ulong nowMs = Time.GetTicksMsec();
        double elapsedSec = (nowMs - _startMs) / 1000.0;

        if (elapsedSec > TimeoutSeconds)
        {
            Fail($"Loading timeout after {TimeoutSeconds:0}s for {_path} (status still InProgress).");
            return;
        }

        var status = ResourceLoader.LoadThreadedGetStatus(_path, _progress);

        float p = 0f;
        if (_progress != null && _progress.Count > 0)
            p = _progress[0].AsSingle();

        // Aktualizuj lastProgress tylko jak faktycznie się zmieniło (Godot czasem raportuje “to samo” długo)
        if (!Mathf.IsEqualApprox(p, _lastProgress))
        {
            _lastProgress = p;
            _lastProgressMs = nowMs;
        }

        _overlay?.SetProgress(p);

        double stallSec = (nowMs - _lastProgressMs) / 1000.0;
        if (stallSec > StallTimeoutSeconds)
        {
            Fail($"Loading stalled for {StallTimeoutSeconds:0}s at progress={p:0.00} for {_path}. Status={status}.");
            return;
        }

        // log co ~1s
        if (_logAccum >= 1.0)
        {
            _logAccum = 0;
            GD.Print($"[LoadingManager] status={status} progress={p:0.00} elapsed={elapsedSec:0.0}s stall={stallSec:0.0}s path={_path}");
        }

        if (status == ResourceLoader.ThreadLoadStatus.InProgress)
            return;

        if (status == ResourceLoader.ThreadLoadStatus.Failed || status == ResourceLoader.ThreadLoadStatus.InvalidResource)
        {
            Fail($"Threaded load failed: {status} for {_path}");
            return;
        }

        if (status == ResourceLoader.ThreadLoadStatus.Loaded)
        {
            // Czasem warto poczekać jedną klatkę przed pobraniem
            var res = ResourceLoader.LoadThreadedGet(_path);

            if (res != null && res is PackedScene packed)
            {
                _overlay?.SetProgress(1f);
                // Używamy Deferred, aby zmiana sceny nie kolidowała z kończącym się procesem ładowania
                GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToPacked, packed);
                CallDeferred(nameof(CleanupAfterSuccess));
            }
            else
            {
                Fail("Resource loaded but is null or not a PackedScene.");
            }
        }
    }

    private void CleanupAfterSuccess()
    {
        _loading = false;
        SetProcess(false);

        if (IsInstanceValid(_overlay))
            _overlay.QueueFree();
        _overlay = null;

        _path = null;
        _progress.Clear();

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void Fail(string msg)
    {
        GD.PushError($"[LoadingManager] {msg}");

        _loading = false;
        SetProcess(false);

        if (IsInstanceValid(_overlay))
            _overlay.QueueFree();
        _overlay = null;

        _path = null;
        _progress.Clear();

        EmitSignal(SignalName.LoadFailed, msg);
    }
}
