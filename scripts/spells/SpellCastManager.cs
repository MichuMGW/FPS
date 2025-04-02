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
    public SpellHand LeftHand {get; set;}
    public SpellHand RightHand {get; set;}
    //Zamienić w przyszłości na kolekcje dla ułatwienia
    public Dictionary<string, Spell> Spells {get; set;} //Currently equiped spells
    public Dictionary<string, Timer> CooldownTimers {get; set;}
    public (Element, Element) CurrentElements {get; set;}

    public override void _Ready()
    {
        //TODO: Zaklęcia powinny być dodawane zależnie od ich odblokowania, zdobycia - pobierane z innej listy
        playerCamera = Owner.GetNode<Camera3D>("Head/Camera3D");

        var mainSpell = new ProjectileSpell(GD.Load<ProjectileSpellResource>(SpellResourcePath.Fireball), (Element.Fire, Element.Fire));
        LeftHand = new SpellHand(mainSpell, Owner.GetNode<MeshInstance3D>("Head/Camera3D/LeftHand"));


        var explosionSpell = new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, (Element.Fire, Element.Fire), "res://scenes/spells/fire_explosion.tscn");
        RightHand = new SpellHand(explosionSpell, Owner.GetNode<MeshInstance3D>("Head/Camera3D/RightHand"));

        CooldownTimers = new Dictionary<string, Timer>{
            {LeftHand.spell.SpellName, InitializeTimer(LeftHand.spell)},
            {RightHand.spell.SpellName, InitializeTimer(RightHand.spell)}
            //TODO: DODAĆ SPECJALNE SPELLE I PRZENIEŚĆ TO DO FUNKCJI
            //DODAĆ SŁOWNIK SPELLI??
        };



        //player.CallDeferred("add_child",explosionSpell);

        // foreach(var spell in leftSpells.Concat<Spell>(rightSpells)){
        //     Timer timer = new Timer();
        //     timer.OneShot = true;
        //     this.AddChild(timer);
        //     cooldownTimers[spell] = timer;
        //     GD.Print(spell.SpellName);
        //     GD.Print("Timer added");
        // }

        // spells.Add(new ExplosionSpell("Fire Explosion", 100, 2.0f, 50, "res://scenes/fire_explosion.tscn"));

    }

    public override void _Process(double delta)
    {
        // //var spell = spells[currentSpell];  
        // //INPUTS - LEFT HAND
        // if (Input.IsActionPressed("CastLeftSpell") && CanCast(leftSpell)){    // && cooldownTimers[spell.SpellName].IsStopped()
        //     CastSpell(leftSpell, leftHand);
        //     isHoldingLeftCast = true;
		// }

        // if (Input.IsActionPressed("CastLeftSpell") && isHoldingLeftCast){
        //     if (leftSpell is ProjectileSpell){
        //         leftSpell.HoldCasting(leftHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, leftHand, delta);
        //     }
        //     else {
        //         leftSpell.HoldCasting(delta);
        //     }
            
        // }

        // if (Input.IsActionJustReleased("CastLeftSpell") && isHoldingLeftCast){
		// 	// var spell = spells[currentSpell];
		// 	leftSpell.EndCasting(leftHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, leftHand);
        //     // cooldownTimers[leftSpell.SpellName].Start();
        //     isHoldingLeftCast = false;
        // }

        // //INPUTS - RIGHT HAND
        // if (Input.IsActionJustPressed("CastRightSpell") && CanCast(rightSpell)){    // && cooldownTimers[spell.SpellName].IsStopped()
		// 	Node3D playerCamera = Owner.GetNode<Camera3D>("Head/Camera3D");
        //     rightSpell.StartCasting(rightHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, rightHand);
        //     isHoldingRightCast = true;
		// }

        // if (Input.IsActionPressed("CastRightSpell") && isHoldingRightCast){
        //     rightSpell.HoldCasting(delta);
        // }

        // if (Input.IsActionJustReleased("CastRightSpell") && isHoldingRightCast){
		// 	// var spell = spells[currentSpell];
		// 	rightSpell.EndCasting(rightHand.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, rightHand);
        //     // cooldownTimers[leftSpell.SpellName].Start();
        //     isHoldingRightCast = false;
        // }

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
    public void SetCooldown(Spell spell, float time){
        CooldownTimers[spell.SpellName].WaitTime = time;
    }

    public void StartCooldown(Spell spell){
        CooldownTimers[spell.SpellName].Start();
    }

    public bool isOnCooldown(Spell spell){
        return !CooldownTimers[spell.SpellName].IsStopped();
    }
    
    public Timer InitializeTimer(Spell spell) {
        Timer timer = new Timer();
        timer.OneShot = true;
        timer.WaitTime = spell.SpellCooldown;
        timer.Timeout += () => {
            // GD.Print($"{spell.SpellName} cooldown finished");
            timer.Stop();
        };
        this.AddChild(timer);
        return timer;
    }

    public void setElements(Element left, Element right){
        CurrentElements = (left, right);
    }

    public void InitializeSpells(){

    }

}
