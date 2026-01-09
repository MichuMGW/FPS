using Godot;
using Microsoft.VisualStudio.TestPlatform.Common;
using System;
using System.Threading.Tasks;

public partial class MainMenuUi : Control
{
    [Export] public string SceneToLoad = "res://scenes/main/main_scene.tscn";
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
        GetTree().ChangeSceneToFile(SceneToLoad);
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
