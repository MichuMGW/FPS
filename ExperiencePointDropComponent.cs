using Godot;
using System;

public partial class ExperiencePointDropComponent : Node
{
	[Export] public PackedScene experiencePoint;

	public override void _Ready()
	{
		HealthComponent healthComponent = GetParent().GetNode<HealthComponent>("EnemyHealthComponent");
		healthComponent.EntityDied += SpawnExperiencePoint;
	}

	public void SpawnExperiencePoint(){
		var spawnPoint = (Owner as Node3D).GlobalPosition;
		var experiencePointInstance = experiencePoint.Instantiate() as Node3D;
		
		GetTree().Root.GetNode("MainScene").AddChild(experiencePointInstance);
		experiencePointInstance.GlobalPosition = spawnPoint;
	}

}
