using Godot;
using System;

public partial class HurtboxArea : Area3D
{
    //MOZNA DODAC KIERUNEK TRAFIENIA
    //(GlobalPosition - source.GlobalPosition).Normalized()
    [Export] public HurtboxType HurtboxType { get; set; } = HurtboxType.Body;
    [Export] public float DamageMultiplier { get; set; } = 1.0f;
    public HurtboxComponent OwnerHurtboxComponent { get; set; }
}
