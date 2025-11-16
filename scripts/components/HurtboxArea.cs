using Godot;
using System;

public partial class HurtboxArea : Area3D
{
    //MOZNA DODAC KIERUNEK TRAFIENIA
    //(GlobalPosition - source.GlobalPosition).Normalized()
    [Export] public HurtboxType HurtboxType { get; set; } = HurtboxType.Body;
    [Export] public float DamageMultiplier { get; set; } = 1.0f;

    public override void _Ready()
    {
        // AddToGroup("hitbox");
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Node3D body)
    {
        //DEBUG
        GD.Print("Enemy hit on " + HurtboxType.GetName(typeof(HurtboxType), HurtboxType));
    }
}
