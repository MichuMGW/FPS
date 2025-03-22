using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpellCastManager : Node
{
    public Dictionary<Spell,Timer> cooldownTimers = new Dictionary<Spell,Timer>();
    // public int currentSpell = 0;
    public Camera3D playerCamera;
    //SPELL IN EACH HAND
    public MeshInstance3D leftHand;
    public MeshInstance3D rightHand;
    public Spell leftSpell;
    public Spell rightSpell;
    public List<Spell> leftSpells = new List<Spell>();
    public List<Spell> rightSpells = new List<Spell>();
    private bool isHoldingLeftCast = false;
    private bool isHoldingRightCast = false;
    
    public override void _Ready()
    {
        //TODO: Zaklęcia powinny być dodawane zależnie od ich odblokowania, zdobycia - pobierane z innej listy
        //Spróbować rozwiązać bez dodawania kolejnych węzłów
        // CharacterBody3D player = GetOwner<CharacterBody3D>();
        playerCamera = Owner.GetNode<Camera3D>("Head/Camera3D");

        //Add reference to player hands
        // leftHand = Owner.GetNode("Head/Camera3D/LeftHand") as MeshInstance3D;

        var projectileSpell = new ProjectileSpell("Fireball", 50, 0.5f, 10, Element.Fire,"res://scenes/projectile.tscn", 20f);
        leftSpells.Add(projectileSpell);
        // player.CallDeferred("add_child",projectileSpell);

        leftHand = Owner.GetNode<MeshInstance3D>("Head/Camera3D/LeftHand");
        leftSpell = projectileSpell;

        var explosionSpell = new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, Element.Fire, "res://scenes/fire_explosion.tscn");
        rightSpells.Add(explosionSpell);

        rightHand = Owner.GetNode<MeshInstance3D>("Head/Camera3D/RightHand");
        rightSpell = explosionSpell;

        //player.CallDeferred("add_child",explosionSpell);

        foreach(var spell in leftSpells.Concat<Spell>(rightSpells)){
            Timer timer = new Timer();
            timer.OneShot = true;
            this.AddChild(timer);
            cooldownTimers[spell] = timer;
            GD.Print(spell.SpellName);
            GD.Print("Timer added");
        }

        // spells.Add(new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, "res://scenes/fire_explosion.tscn"));

    }

    public override void _Process(double delta)
    {
        //var spell = spells[currentSpell];  
        //INPUTS - LEFT HAND
        if (Input.IsActionPressed("CastLeftSpell") && CanCast(leftSpell)){    // && cooldownTimers[spell.SpellName].IsStopped()
            CastSpell(leftSpell, leftHand);
            isHoldingLeftCast = true;
		}

        if (Input.IsActionPressed("CastLeftSpell") && isHoldingLeftCast){
            if (leftSpell is ProjectileSpell){
                leftSpell.HoldCasting(leftHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, leftHand, delta);
            }
            else {
                leftSpell.HoldCasting(delta);
            }
            
        }

        if (Input.IsActionJustReleased("CastLeftSpell") && isHoldingLeftCast){
			// var spell = spells[currentSpell];
			leftSpell.EndCasting(leftHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, leftHand);
            // cooldownTimers[leftSpell.SpellName].Start();
            isHoldingLeftCast = false;
        }

        //INPUTS - RIGHT HAND
        if (Input.IsActionJustPressed("CastRightSpell") && CanCast(rightSpell)){    // && cooldownTimers[spell.SpellName].IsStopped()
			Node3D playerCamera = Owner.GetNode<Camera3D>("Head/Camera3D");
            rightSpell.StartCasting(rightHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, rightHand);
            isHoldingRightCast = true;
		}

        if (Input.IsActionPressed("CastRightSpell") && isHoldingRightCast){
            rightSpell.HoldCasting(delta);
        }

        if (Input.IsActionJustReleased("CastRightSpell") && isHoldingRightCast){
			// var spell = spells[currentSpell];
			rightSpell.EndCasting(rightHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, rightHand);
            // cooldownTimers[leftSpell.SpellName].Start();
            isHoldingRightCast = false;
        }

        // if(Input.IsActionJustPressed("NextLeftSpell")){
        //     if(spells.Count - 1 == currentSpell){
        //         currentSpell = 0;
        //     }
        //     else {
        //         currentSpell++;
        //     }
        // }
        // if(Input.IsActionJustPressed("PreviousSpell")){
        //     if(currentSpell == 0){
        //         currentSpell = spells.Count - 1;
        //     }
        //     else {
        //         currentSpell--;
        //     }
        // }
        

        
        //INPUTS
    }

    private bool CanCast(Spell spell){
        return cooldownTimers[spell].IsStopped();
    }

    private void CastSpell(Spell spell, Node3D hand){
        spell.StartCasting(hand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, hand);
        cooldownTimers[spell].Start(spell.SpellCooldown);
    }

}
