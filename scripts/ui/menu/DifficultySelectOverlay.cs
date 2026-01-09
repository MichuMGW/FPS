using Godot;
using System;

public partial class DifficultySelectOverlay : Control
{
    [Signal] public delegate void DifficultyChosenEventHandler(int difficulty);
    [Signal] public delegate void CanceledEventHandler();

    [Export] private Button casualButton;
    [Export] private Button normalButton;
    [Export] private Button hardcoreButton;
    [Export] private Button backButton;

    public override void _Ready()
    {
        casualButton.Pressed += () => EmitSignal(nameof(DifficultyChosen), (int)RunDifficulty.Casual);
        normalButton.Pressed += () => EmitSignal(nameof(DifficultyChosen), (int)RunDifficulty.Normal);
        hardcoreButton.Pressed += () => EmitSignal(nameof(DifficultyChosen), (int)RunDifficulty.Hardcore);

        if (backButton != null)
            backButton.Pressed += () =>
            {
                EmitSignal(nameof(Canceled));
                QueueFree();
            };
    }
}
