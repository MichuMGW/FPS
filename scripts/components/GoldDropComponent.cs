using Godot;

public partial class GoldDropComponent : Node
{
    [Export] public int BaseGold = 3;

    private Enemy _owner;
    private HealthComponent _health;
    private GoldManager _gold;

    public override void _Ready()
    {
        _owner = GetParent() as Enemy;
        _health = _owner.GetNodeOrNull<HealthComponent>("HealthComponent");
        _gold = GetTree().Root.GetNodeOrNull<GoldManager>("GoldManager");

        if (_health == null || _gold == null)
        {
            GD.PushWarning("[GoldDropComponent] Missing dependencies.");
            return;
        }

        _health.EntityDied += OnDied;
    }

    private void OnDied()
    {
        var diff = _owner.CurrentDifficulty;

        float coeff = Mathf.Max(1f, diff.Coeff);

        int scaledGold = Mathf.RoundToInt(
            BaseGold * Mathf.Sqrt(coeff)
        );

        scaledGold += diff.EnemyLevel / 2;

        _gold.AddGold(Mathf.Max(1, scaledGold));
    }
}
