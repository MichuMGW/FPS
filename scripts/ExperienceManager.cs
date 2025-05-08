using Godot;
using System;

public partial class ExperienceManager : Node
{
	[Signal]
	public delegate void LevelUpEventHandler(int experience);

	private GameEvents gameEvents;

	private int currentExperience = 0;
	private int experienceToNextLevel = 100;
	private int currentLevel = 1;
	private int NEXT_LEVEL_GROWTH = 100;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameEvents = GetNode<GameEvents>("/root/GameEvents");
		gameEvents.ExperiencePointCollected += OnExperiencePointCollected;
	}

	public void OnExperiencePointCollected(int experience){
		currentExperience += experience;
		if(currentExperience >= experienceToNextLevel){
			currentExperience -= experienceToNextLevel;
			currentLevel++;
			experienceToNextLevel = experienceToNextLevel + NEXT_LEVEL_GROWTH;
			EmitSignal(nameof(LevelUpEventHandler), currentLevel);
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {

	// }
}
