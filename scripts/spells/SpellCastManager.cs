using Godot;
using System;
using System.Collections.Generic;

public partial class SpellCastManager : Node
{
	public List<Spell> spells = new List<Spell>();
    public int currentSpell = 0;
    
    public override void _Ready()
    {
        spells.Add(new ProjectileSpell("Fireball", 50, 2.0f, 10, "res://scenes/projectile.tscn", 20f));
        spells.Add(new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, "res://scenes/fire_explosion.tscn"));
    }

    public override void _Input(InputEvent @event)
    {
        
    }

    public override void _Process(double delta)
    {
        //INPUTS

        if (Input.IsActionJustPressed("CastSpell")){
			CharacterBody3D player = GetOwner<CharacterBody3D>();
			Node3D playerCamera = player.GetNode<Camera3D>("Head/Camera3D");
			var spell = spells[currentSpell];
			spell.Cast(player.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, player);
		}

        if(Input.IsActionJustPressed("NextSpell")){
            if(spells.Count - 1 == currentSpell){
                currentSpell = 0;
            }
            else {
                currentSpell++;
            }
        }
        if(Input.IsActionJustPressed("PreviousSpell")){
            if(currentSpell == 0){
                currentSpell = spells.Count - 1;
            }
            else {
                currentSpell--;
            }
        }
        //INPUTS
    }
}
