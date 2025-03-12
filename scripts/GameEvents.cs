using Godot;
using System;

public partial class GameEvents : Node
{
	[Signal]
	public delegate void ExperiencePointCollectedEventHandler(int currentExperience);
	[Signal]
	public delegate void MenuEnabledEventHandler(bool isMenuEnabled);

	public PackedScene gameMenuScene;
	public Control gameMenuInstance;
	public bool isMenuEnabled = false;
	private Node mainScene;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		gameMenuScene = GD.Load<PackedScene>("res://scenes/game_menu_scene.tscn");
		mainScene = GetTree().CurrentScene;
	}

	public void EmitExperiencePointCollected(int currentExperience)
	{
		EmitSignal(nameof(ExperiencePointCollected), currentExperience);
		//GD.Print("Experience Point Collected");
	}

	//Może do usunięcia
	public void EmitMenuEnabled(bool isMenuEnabled)
	{
		EmitSignal(nameof(MenuEnabled), isMenuEnabled);
		this.isMenuEnabled = isMenuEnabled;
	}

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_cancel") && GetTree().CurrentScene.Name == "MainScene"){
			if(!isMenuEnabled)
			{
				gameMenuInstance = (Control)gameMenuScene.Instantiate();
				GetTree().Paused = true;
				
				GetTree().Root.AddChild(gameMenuInstance);
				EmitMenuEnabled(true);
			} 
			else 
			{
				GetTree().Paused = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;

				gameMenuInstance.QueueFree();
				EmitMenuEnabled(false);
			}
		}
    }


}
