using Godot;
using Microsoft.VisualStudio.TestPlatform.Common;
using System;
using System.Threading.Tasks;

public partial class MainMenuUi : Control
{
    [Export] public string SceneToLoad = "res://scenes/main/main_scene.tscn";
    [Export] public PackedScene LoadingOverlayScene;
    [Export] public PackedScene DifficultyOverlayScene;

    private RunConfig _runConfig;

    [Export] private Button startButton;
    [Export] private Button settingsButton;
    [Export] private Button quitButton;

    private bool _isLoading;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        _runConfig = GetTree().Root.GetNode<RunConfig>("RunConfig");
        if (_runConfig == null)
            GD.PushWarning("[MainMenuUi] GameEvents not found.");

        startButton.Pressed += OnStartButtonPressed;
        settingsButton.Pressed += OnSettingsButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private async void OnStartButtonPressed()
    {
        if (_isLoading) return;
        _isLoading = true;

        startButton.Disabled = true;
        settingsButton.Disabled = true;
        quitButton.Disabled = true;

        // 1) Wybór trudności
        var chosen = await ShowDifficultyPicker();
        if (chosen == null)
        {
            // Anulowano
            startButton.Disabled = false;
            settingsButton.Disabled = false;
            quitButton.Disabled = false;
            _isLoading = false;
            return;
        }

        _runConfig.SetDifficulty(chosen.Value);

        // 2) Dopiero teraz ładujesz grę
        await LoadGameAsync();
    }

    private async Task<RunDifficulty?> ShowDifficultyPicker()
    {
        if (DifficultyOverlayScene == null)
        {
            GD.PushWarning("[MainMenuUi] DifficultyOverlayScene not assigned. Using Normal.");
            return RunDifficulty.Normal;
        }

        var overlay = DifficultyOverlayScene.Instantiate<DifficultySelectOverlay>();
        AddChild(overlay);

        var tcs = new TaskCompletionSource<RunDifficulty?>();

        overlay.DifficultyChosen += (int d) =>
        {
            tcs.TrySetResult((RunDifficulty)d);
            overlay.QueueFree();
        };

        overlay.Canceled += () =>
        {
            tcs.TrySetResult(null);
            overlay.QueueFree();
        };

        return await tcs.Task;
    }

    private async Task LoadGameAsync()
    {
        var tree = GetTree(); // cache, bo po ChangeScene menu wypada z drzewa

        LoadingOverlay loading = null;
        if (LoadingOverlayScene != null)
        {
            loading = LoadingOverlayScene.Instantiate<LoadingOverlay>();
            AddChild(loading);
            loading.SetProgress(0f);
        }

        var err = ResourceLoader.LoadThreadedRequest(SceneToLoad);
        if (err != Error.Ok)
        {
            FailLoad($"LoadThreadedRequest failed: {err} for {SceneToLoad}", loading);
            return;
        }

        var progress = new Godot.Collections.Array();

        while (true)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(SceneToLoad, progress);

            float p = 0f;
            if (progress.Count > 0)
                p = progress[0].AsSingle();

            loading?.SetProgress(p);

            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
                break;

            if (status == ResourceLoader.ThreadLoadStatus.Failed ||
                status == ResourceLoader.ThreadLoadStatus.InvalidResource)
            {
                FailLoad($"Threaded load failed. Status: {status}, path: {SceneToLoad}", loading);
                return;
            }

            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        var packed = ResourceLoader.LoadThreadedGet(SceneToLoad) as PackedScene;
        if (packed == null)
        {
            FailLoad($"Loaded resource is not a PackedScene: {SceneToLoad}", loading);
            return;
        }

        loading?.SetProgress(1f);

        Input.MouseMode = Input.MouseModeEnum.Captured;

        var changeErr = tree.ChangeSceneToPacked(packed);
        if (changeErr != Error.Ok)
        {
            FailLoad($"ChangeSceneToPacked failed: {changeErr}", loading);
            return;
        }

        // Tu już nie awaituj na menu, bo menu zaraz zostanie wyjebane z drzewa.
        // StartGame odpal w scenie gry w _Ready().
    }

    private void FailLoad(string msg, LoadingOverlay overlay)
    {
        GD.PushError($"[MainMenuUi] {msg}");
        overlay?.QueueFree();

        startButton.Disabled = false;
        settingsButton.Disabled = false;
        quitButton.Disabled = false;

        _isLoading = false;
    }

    private void OnSettingsButtonPressed() => GD.Print("Settings Button Pressed");
    private void OnQuitButtonPressed() => GetTree().Quit();
}
