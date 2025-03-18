using Godot;
using System;
using System.Collections.Generic;

public partial class SpellCastManager : Node
{
	public List<Spell> spells = new List<Spell>();
    public Dictionary<string,Timer> cooldownTimers = new Dictionary<string,Timer>();
    public int currentSpell = 0;
    private bool isHoldingCast = false;
    
    public override void _Ready()
    {
        //TODO: Zaklęcia powinny być dodawane zależnie od ich odblokowania, zdobycia - pobierane z innej listy
        //Spróbować rozwiązać bez dodawania kolejnych węzłów
        CharacterBody3D player = GetOwner<CharacterBody3D>();

        var projectileSpell = new ProjectileSpell("Fireball", 50, 2.0f, 10, "res://scenes/projectile.tscn", 20f);
        spells.Add(projectileSpell);
        player.CallDeferred("add_child",projectileSpell);

        var explosionSpell = new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, "res://scenes/fire_explosion.tscn");
        spells.Add(explosionSpell);
        player.CallDeferred("add_child",explosionSpell);

        foreach(var spell in spells){
            Timer timer = new Timer();
            timer.OneShot = true;
            this.AddChild(timer);
            cooldownTimers.Add(spell.SpellName, timer);
            GD.Print(spell.SpellName);
            GD.Print("Timer added");
        }

        

        // spells.Add(new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, "res://scenes/fire_explosion.tscn"));


    }

    public override void _Process(double delta)
    {
        //INPUTS
        var spell = spells[currentSpell];
        
        if (Input.IsActionJustPressed("CastSpell") && cooldownTimers[spell.SpellName].IsStopped()){
			CharacterBody3D player = GetOwner<CharacterBody3D>();
			Node3D playerCamera = player.GetNode<Camera3D>("Head/Camera3D");
            spell.StartCasting(player.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, player);
            isHoldingCast = true;
		}

        if (Input.IsActionPressed("CastSpell") && isHoldingCast){
            // var spell = spells[currentSpell];
            spell.HoldCasting(delta);
        }

        if (Input.IsActionJustReleased("CastSpell") && isHoldingCast){
            CharacterBody3D player = GetOwner<CharacterBody3D>();
			Node3D playerCamera = player.GetNode<Camera3D>("Head/Camera3D");
			// var spell = spells[currentSpell];
			spell.EndCasting(player.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, player);
            cooldownTimers[spell.SpellName].Start();
            isHoldingCast = false;
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
