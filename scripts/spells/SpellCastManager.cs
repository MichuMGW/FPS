using Godot;
using System;
using System.Collections.Generic;

public partial class SpellCastManager : Node
{
	public List<Spell> spells = new List<Spell>();

    public override void _Ready()
    {
        spells.Add(new ProjectileSpell());
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("CastSpell")){
			CharacterBody3D player = GetOwner<CharacterBody3D>();
			Node3D playerCamera = player.GetNode<Camera3D>("Head/Camera3D");
			var spell = spells[0];
			spell.Cast(player.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, player);
		}
    }
}
