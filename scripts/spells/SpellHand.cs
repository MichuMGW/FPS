using Godot;
using System;

public partial class SpellHand : Node {
    public Spell spell;
    public MeshInstance3D handMesh;
    public bool isHoldingCast = false;
    public bool isOnCooldown = false;
    public RayCast3D raycast;
    public SpellHand(Spell spell, MeshInstance3D handMesh){
        this.spell = spell;
        this.handMesh = handMesh;

        raycast = handMesh.GetParent().GetNode<RayCast3D>("RayCast3D");
        if (raycast == null) {
            GD.Print("Player camera not found in SpellHand constructor");
        }
        // cooldownTimer.OneShot = true;
        // cooldownTimer.WaitTime = spell.SpellCooldown;
        // cooldownTimer.Timeout += OnCooldownTimeout;
    }
    public void StartCasting(){
        
        if (isOnCooldown) return;
        Vector3 direction;
        if (raycast.IsColliding()) {
            direction = raycast.GetCollisionPoint() - handMesh.GlobalTransform.Origin;
            direction = direction.Normalized();
        } else {
            var targetPosition = raycast.GlobalTransform.Origin + (-raycast.GlobalTransform.Basis.Z * spell.SpellRange);
            direction = (targetPosition - handMesh.GlobalTransform.Origin).Normalized();
        }
        spell.StartCasting(handMesh.GlobalTransform.Origin, direction, handMesh);
        // StartCooldown();
        //Tutaj można dodać animację dla początku castowania spella
    }

}

