using Godot;

public partial class ExperiencePickup : Node3D
{
	public CharacterBody3D player;
	public Area3D area;
	private float _speed = 0.1f;
	private bool _isInRadius = false;
	[Export] public int Amount { get; set; } = 10;
	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.AreaEntered += OnAreaEntered;

		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isInRadius)
		{
			MoveTowardsPlayer(delta);
		}
	}
	
	private void MoveTowardsPlayer(double delta){
		player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
		if(player == null){
			return;
		}
		Vector3 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		_speed += _speed * (float)delta;
		Vector3 newVelocity =  direction * _speed; 
		Translate(newVelocity);
	}

	public void OnAreaEntered(Area3D area)
	{
		if (area.Name == "PickupArea")
		{
			var Exp = GetTree().Root.GetNodeOrNull<ExperienceManager>("ExperienceManager");
			Exp.AddExp(Amount);
			QueueFree();
		}
		if (area.Name == "PickupRadiusArea")
		{
			_isInRadius = true;
			SetPhysicsProcess(true);
		}
	}
}
