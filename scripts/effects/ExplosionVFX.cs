using Godot;
using System;

public partial class ExplosionVFX : Node3D
{
    private AnimationPlayer _anim;
    public override void _Ready()
    {
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");   
    }
    public void Explode()
    {
        _anim.Play("Explode");
    }
}
