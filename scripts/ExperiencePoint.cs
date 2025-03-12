using Godot;
using System;

public partial class ExperiencePoint : Node3D
{
	public CharacterBody3D player;
	public Area3D area;
	private float speed = 0.1f;
	private bool isInRadius = false;
	private GameEvents gameEvents;
	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.AreaEntered += OnAreaEntered;

		gameEvents = GetNode<GameEvents>("/root/GameEvents");
	}

	public override void _Process(double delta)
	{
		if(isInRadius)
		{
			player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
			Vector3 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			speed += speed * (float)delta;
			Vector3 newVelocity =  direction * speed; 
			Translate(newVelocity);

		}
	}

	public void OnAreaEntered(Area3D area)
	{
		if (area.Name == "PickupArea")
		{
			gameEvents.EmitExperiencePointCollected(10);
			QueueFree();
		}
		if (area.Name == "PickupRadiusArea")
		{
			isInRadius = true;
		}
	}
}
