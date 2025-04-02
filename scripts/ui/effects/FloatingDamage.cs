using Godot;
using System;

public partial class FloatingDamage : Node3D
{
	[Export] private float lifeTime = 1.0f;
	[Export] private float moveSpeed = 2f;
	[Export] private float oscillationAmplitude = 0.3f;
	[Export] private float oscillationSpeed = 5.0f;
	private float randomOffset;
	private Label3D damageLabel;
	private float timeElapsed = 0f;
	private Camera3D camera;

    public override void _Ready()
    {
        damageLabel = GetNode<Label3D>("Label3D");
		// randomOffset = GD.Randf() * 2 * Mathf.Pi;
		// camera = GetViewport().GetCamera3D();
    }

	// public override void _Process(double delta)
	// {
	// 	LookAt(-camera.GlobalPosition, Vector3.Up);
	// 	timeElapsed += (float)delta;
	// 	float xOffset = Mathf.Sin(timeElapsed * oscillationSpeed + randomOffset) * oscillationAmplitude;
	// 	GlobalPosition = new Vector3(startPosition.X + xOffset, GlobalPosition.Y, startPosition.Z);
	// }

	public void SetDamage(float damage){
		damageLabel.Text = damage.ToString();
	}

	public void ShowDamage(float damage){
		double offsetX = GD.RandRange(-0.5f, 0.5f);
		double offsetZ = GD.RandRange(-0.5f, 0.5f);
		Position += new Vector3((float)offsetX, 0, (float)offsetZ);
		var tween = GetTree().CreateTween();
		tween.Parallel().TweenProperty(damageLabel, "position:y", Position.Y + moveSpeed, lifeTime);
		//Można dodać oscylacje na boki jakimś sinusem
		tween.Parallel().TweenProperty(damageLabel, "modulate:a", 0.0f, lifeTime);
		// tween.Parallel().TweenProperty(damageLabel, "position:x",  )
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}
}
