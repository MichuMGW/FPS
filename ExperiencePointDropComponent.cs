using Godot;
using System;

public partial class ExperiencePointDropComponent : Node
{
	[Export]
	public PackedScene experiencePoint;
	[Export]
	public HealthComponent healthComponent;

	public override void _Ready()
	{
		healthComponent.EntityDied += SpawnExperiencePoint;
	}

	public void SpawnExperiencePoint(){
		var spawnPoint = (Owner as Node3D).GlobalPosition;
		var experiencePointInstance = experiencePoint.Instantiate() as Node3D;
		
		GetTree().Root.GetNode("MainScene").AddChild(experiencePointInstance);
		experiencePointInstance.GlobalPosition = spawnPoint;
	}

}
