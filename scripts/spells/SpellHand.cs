using Godot;
using System;

public partial class SpellHand : Node {
    public Spell spell;
    public Node3D hand;
    public bool isHoldingCast = false;
    public bool isOnCooldown = false;
    public RayCast3D raycast;
    public ArmsAnimationPlayer animationPlayer;
    public SpellHand(Spell spell, Node3D hand){
        this.spell = spell;
        this.hand = hand;
        var player = hand.GetTree().GetFirstNodeInGroup("player") as Player;
        raycast = player.GetNode<RayCast3D>("Head/Camera3D/RayCast3D");
        animationPlayer = player.GetNode<ArmsAnimationPlayer>("Head/Camera3D/Arms/AnimationPlayer");
        // raycast = hand.GetNode<RayCast3D>("RayCast3D");
        if (raycast == null) {
            GD.Print("Player raycast not found in SpellHand constructor");
        }
        // cooldownTimer.OneShot = true;
        // cooldownTimer.WaitTime = spell.SpellCooldown;
        // cooldownTimer.Timeout += OnCooldownTimeout;
    }
    public void StartCasting(){
        
        if (isOnCooldown) return;
        Vector3 direction;
        if (raycast.IsColliding()) {
            direction = (raycast.GetCollisionPoint() - hand.GlobalTransform.Origin).Normalized();
            
        } else {
            var targetPosition = raycast.GlobalTransform.Origin + (-raycast.GlobalTransform.Basis.Z * spell.SpellRange);
            direction = (targetPosition - hand.GlobalTransform.Origin).Normalized();
        }

        spell.StartCasting(hand.GlobalTransform.Origin, direction, hand);
        // StartCooldown();
        //Tutaj można dodać animację dla początku castowania spella
    }

}

