using Godot;
using System;

public partial class ProjetileCast : State
{
	public SpellCastManager spm;
    public override void Enter()
    {
		SpellCastManager spm = GetParent() as SpellCastManager;
        CharacterBody3D player = GetOwner<CharacterBody3D>();
		Node3D playerCamera = player.GetNode<Camera3D>("Head/Camera3D");
		var spell = spm.spells[spm.currentSpell];
		spell.StartCasting(player.GlobalTransform.Origin, -playerCamera.GlobalTransform.Basis.Z, player);
    }

    public override void Exit()
    {
        
    }

    public override void Physics_Update(double delta)
    {
        
    }

    public override void Update(double delta)
    {
        
    }
}
