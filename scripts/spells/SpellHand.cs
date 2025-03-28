using Godot;
using System;

public partial class SpellHand : Node {
    public Spell spell;
    public MeshInstance3D handMesh;
    public bool isHoldingCast = false;
    public bool isOnCooldown = false;
    public Camera3D playerCamera;
    public SpellHand(Spell spell, MeshInstance3D handMesh){
        this.spell = spell;
        this.handMesh = handMesh;

        playerCamera = handMesh.GetParent() as Camera3D;
        if (playerCamera == null) {
            GD.Print("Player camera not found in SpellHand constructor");
        }
        // cooldownTimer.OneShot = true;
        // cooldownTimer.WaitTime = spell.SpellCooldown;
        // cooldownTimer.Timeout += OnCooldownTimeout;
    }
    public void StartCasting(){
        if (isOnCooldown) return;
        spell.StartCasting(handMesh.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, handMesh);
        // StartCooldown();
        //Tutaj można dodać animację dla początku castowania spella
    }

}

