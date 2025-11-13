using Godot;
using System;

public partial class HitboxArea : Area3D
{
    //MOZNA DODAC KIERUNEK TRAFIENIA
    //(GlobalPosition - source.GlobalPosition).Normalized()
    [Export] public HitboxType HitboxType { get; set; } = HitboxType.Body;
    [Export] public float DamageMultiplier { get; set; } = 1.0f;

    public override void _Ready()
    {
        AddToGroup("hitbox");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        //Do debugowania
    }
}
