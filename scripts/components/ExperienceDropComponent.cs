using Godot;

public partial class ExperienceDropComponent : Node
{
    [Export] public PackedScene PickupScene;
    [Export] public int BaseXp = 5;

    private Enemy _owner;
    private HealthComponent _health;

    public override void _Ready()
    {
        _owner = GetParent() as Enemy;
        _health = _owner.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (_health == null)
        {
            GD.PushWarning("[ExperienceDropComponent] Missing HealthComponent on parent.");
            return;
        }

        _health.EntityDied += OnDied;
    }

    private void OnDied()
    {
        if (PickupScene == null) return;

        var pickup = PickupScene.Instantiate() as ExperiencePickup;
        GetTree().CurrentScene.AddChild(pickup);

        // parent powinien być Node3D / CharacterBody3D
        if (GetParent() is Node3D parent3D)
        {
            var position = parent3D.GlobalPosition;
            position.Y += 1.0f;
            pickup.GlobalPosition = position;
        }

        pickup.Amount = Mathf.RoundToInt(BaseXp * _owner.CurrentDifficulty.EnemyLevel); // tu później difficulty scaling
    }
}
