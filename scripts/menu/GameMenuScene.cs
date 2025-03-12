using Godot;
using System;

public partial class GameMenuScene : Control
{
	[Export]
	public Button continueButton;
	[Export]
	public Button settingsButton;
	[Export]
	public Button exitButton;
	public string SceneToLoad = "res://scenes/main_menu.tscn";

	public GameEvents gameEvents;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.WhenPaused;

		gameEvents = GetNode<GameEvents>("/root/GameEvents");

		continueButton.Pressed += OnContinueButtonPressed;
		settingsButton.Pressed += OnSettingsButtonPressed;
		exitButton.Pressed += OnExitButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void OnContinueButtonPressed()
	{
		gameEvents.EmitMenuEnabled(false);
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		QueueFree();
	}
	public void OnSettingsButtonPressed()
	{
		GD.Print("Settings Button Pressed");
	}
	public void OnExitButtonPressed()
	{
		GetTree().Paused = false;
		gameEvents.EmitMenuEnabled(false);
		GetTree().ChangeSceneToFile(SceneToLoad);
		QueueFree();
	}


}
