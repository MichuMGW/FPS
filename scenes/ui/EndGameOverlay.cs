using Godot;
using System;
using System.Threading.Tasks;

public partial class EndGameOverlay : CanvasLayer
{
    [Export] public NodePath DimmerPath = "ColorRect";
    [Export] public NodePath ContentPath = "MarginContainer";

    // Tytuł / status
    [Export] public NodePath GameFinishedLabelPath = "MarginContainer/VBoxContainer/GameFinished";

    // GridContainer labelki (po lewej nazwy, po prawej wartości)
    [Export] public NodePath DifficultyScorePath = "MarginContainer/VBoxContainer/GridContainer/DifficultyScore";
    [Export] public NodePath EnemiesKilledScorePath = "MarginContainer/VBoxContainer/GridContainer/EnemiesKilledScore";
    [Export] public NodePath GoldCollectedScorePath = "MarginContainer/VBoxContainer/GridContainer/GoldCollectedScore";
    [Export] public NodePath PlayerLevelScorePath = "MarginContainer/VBoxContainer/GridContainer/PlayerLevelScore";
    [Export] public NodePath RunDurationScorePath = "MarginContainer/VBoxContainer/GridContainer/RunDurationScore";

    // Final score
    [Export] public NodePath FinalScorePath = "MarginContainer/VBoxContainer/HBoxContainer/FinalScore";

    [Export] public float FadeToAlpha = 0.8f;
    [Export] public float FadeSeconds = 0.6f;
    [Export] public float ShowDurationSeconds = 15f;

    [Export(PropertyHint.File, "*.tscn")]
    public string MainMenuScenePath = "res://scenes/ui/menu/main_menu.tscn";

    private ColorRect _dimmer;
    private Control _content;

    private Label _gameFinished;
    private Label _difficultyScore;
    private Label _enemiesScore;
    private Label _goldScore;
    private Label _levelScore;
    private Label _durationScore;
    private Label _finalScore;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        AddToGroup("overlay"); // żeby PauseManager/ESC nie mieszał

        _dimmer = GetNodeOrNull<ColorRect>(DimmerPath);
        _content = GetNodeOrNull<Control>(ContentPath);

        _gameFinished = GetNodeOrNull<Label>(GameFinishedLabelPath);

        _difficultyScore = GetNodeOrNull<Label>(DifficultyScorePath);
        _enemiesScore = GetNodeOrNull<Label>(EnemiesKilledScorePath);
        _goldScore = GetNodeOrNull<Label>(GoldCollectedScorePath);
        _levelScore = GetNodeOrNull<Label>(PlayerLevelScorePath);
        _durationScore = GetNodeOrNull<Label>(RunDurationScorePath);

        _finalScore = GetNodeOrNull<Label>(FinalScorePath);

        if (_content != null) _content.Visible = false;

        // start dimmera: alpha=0
        if (_dimmer != null)
        {
            var c = _dimmer.Color;
            c.A = 0f;
            _dimmer.Color = c;
            _dimmer.Visible = true;
        }

        // dla UI na koniec gry zazwyczaj chcesz myszkę
        Input.MouseMode = Input.MouseModeEnum.Visible;

        // odpal sekwencję
        _ = RunSequence();
    }

    private async Task RunSequence()
    {
        // 1) Fade in dimmera
        await FadeDimmerIn();

        // 2) Wypełnij wyniki i pokaż content
        FillScores();
        if (_content != null) _content.Visible = true;

        // 3) Czekaj 15 sekund (od momentu pokazania contentu)
        await WaitSeconds(ShowDurationSeconds);

        // 4) Powrót do menu
        GoToMainMenu();
    }

    private async Task FadeDimmerIn()
    {
        if (_dimmer == null)
            return;

        // Tween alpha w ColorRect.Color.A
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Sine);

        var from = _dimmer.Color;
        var to = _dimmer.Color;
        to.A = Mathf.Clamp(FadeToAlpha, 0f, 1f);

        _dimmer.Color = from;
        tween.TweenProperty(_dimmer, "color", to, Mathf.Max(0.01f, FadeSeconds));

        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private void FillScores()
    {
        // Spróbuj wziąć wynik z GameScore
        GameScore scoreNode = GetTree().Root.GetNodeOrNull<GameScore>("GameScore");
        if (scoreNode == null)
            scoreNode = GetTree().CurrentScene?.GetNodeOrNull<GameScore>("GameScore");

        GameScore.Result r = scoreNode?.LastResult;

        // fallback, jeśli LastResult nie ustawione (bo ktoś zapomniał FreezeScore)
        if (r == null && scoreNode != null)
            r = scoreNode.GetLiveSnapshot();

        // jak nie ma nic, to chociaż nie wywal gry
        if (r == null)
        {
            GD.PushWarning("[EndGameOverlay] Missing GameScore result. Filling placeholders.");
            SetLabel(_difficultyScore, "-");
            SetLabel(_enemiesScore, "0");
            SetLabel(_goldScore, "0");
            SetLabel(_levelScore, "1");
            SetLabel(_durationScore, "00:00");
            SetLabel(_finalScore, "0");
            return;
        }

        // Teksty
        if (_gameFinished != null)
        {
            _gameFinished.Text = r.Reason == RunEndReason.PlayerDied
                ? "YOU DIED"
                : "RUN FINISHED";
        }

        SetLabel(_difficultyScore, r.Difficulty ?? "-");
        SetLabel(_enemiesScore, r.EnemiesKilled.ToString());
        SetLabel(_goldScore, r.GoldCollected.ToString());
        SetLabel(_levelScore, r.PlayerLevel.ToString());

        // czas w mm:ss
        var t = Mathf.Max(0f, r.RunTimeSeconds);
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        SetLabel(_durationScore, $"{m:00}:{s:00}");

        // Final score (prosty przykład, Ty sobie to potem wyregulujesz)
        int final = CalculateFinalScore(r);
        SetLabel(_finalScore, final.ToString());
    }

    private int CalculateFinalScore(GameScore.Result r)
    {
        // To jest „sensowny default”, a nie religia.
        // Jak będziesz chciał, dorobimy mnożniki per difficulty itd.
        int score =
            r.EnemiesKilled * 10 +
            r.GoldCollected * 1 +
            r.ItemsCollected * 25 +
            r.PlayerLevel * 50;

        if (r.Reason == RunEndReason.RunFinished)
            score += 500;

        return Math.Max(0, score);
    }

    private void SetLabel(Label l, string text)
    {
        if (l != null) l.Text = text ?? "";
    }

    private async Task WaitSeconds(float seconds)
    {
        // Timer działa niezależnie od fps, a jak ktoś ma pauzę w tle, to i tak overlay jest Always
        var timer = GetTree().CreateTimer(Mathf.Max(0.01f, seconds), processAlways: true);
        await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
    }

    private void GoToMainMenu()
    {
        var tree = GetTree();
        if (tree == null) return;

        tree.Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (string.IsNullOrWhiteSpace(MainMenuScenePath))
        {
            GD.PushWarning("[EndGameOverlay] MainMenuScenePath is empty.");
            return;
        }

        QueueFree();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, MainMenuScenePath);
    }
}
