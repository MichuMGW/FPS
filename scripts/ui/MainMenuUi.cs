using Godot;
using System;

public partial class MainMenuUi : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public string SceneToLoad = "res://scenes/main_scene.tscn";
	public GameEvents _events;

	//Buttons
	[Export]
	private Button startButton;
	[Export]
	private Button settingsButton;
	[Export]
	private Button quitButton;
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_events = GetTree().GetFirstNodeInGroup("game_events") as GameEvents;

		startButton.Pressed += OnStartButtonPressed;
		settingsButton.Pressed += OnSettingsButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed; 
	}

	private void OnStartButtonPressed()
	{
		_events.StartGame();
		GetTree().ChangeSceneToFile(SceneToLoad); //PRZENIEŚĆ DO GAME EVENTS
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnSettingsButtonPressed()
	{
		GD.Print("Settings Button Pressed");
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}


}
