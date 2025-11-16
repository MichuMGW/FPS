using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

//Komponent obsługujący hitboxy bytu. Korzysta z HurtboxArea, które zczytują trafienia dla konkretnych kości.
public partial class HurtboxComponent : Node
{
	[Signal] public delegate void HitEventHandler(HitInfo hitInfo);
	public List<HurtboxArea> hurtboxes {get; set;}
	private readonly Dictionary<Node3D, double> _lastHitTimeBySource = new();
	[Export] string armaturePath {get; set;}
	//ZMIENIĆ JAK BĘDĘ ROBIŁ BEAM SPELLE TO ZMIENIĆ NA MNIEJSZĄ WARTOŚĆ
	[Export] public float RehitCooldownSeconds { get; set; } = 0f;

	public override void _Ready()
	{
		hurtboxes = new List<HurtboxArea>();
		var armature = GetParent().GetNode<Skeleton3D>(armaturePath);

		foreach(Node3D node in armature.GetChildren())
		{
			if(node is BoneAttachment3D boneAttachment)
			{
				hurtboxes.Add(boneAttachment.GetNode<HurtboxArea>("HurtboxArea"));
			}
		}

		foreach(HurtboxArea hitbox in hurtboxes)
		{
			//Body/source to obiekt, który wszedł w HurtboxArea
			hitbox.AreaEntered += source => OnHurtboxAreaEntered(hitbox, source);
		}
	}

	public void OnHurtboxAreaEntered(HurtboxArea hitbox, Node3D source)
	{
		// if (source is not IDamageSource damage)
		// {
		// 	return;
		// }

		// Node3D rootTarget = GetOwner<Node3D>();
		// if (!damage.CanHitAgain(rootTarget))
		// {
		// 	return;
		// }

		// damage.RegisterHit(rootTarget);

		var hitInfo = new HitInfo
		{
			Source = source,
			HitboxType = hitbox.HurtboxType,
			DamageMultiplier = hitbox.DamageMultiplier,
			HitPosition = hitbox.GlobalPosition
		};

		EmitSignal(nameof(Hit), hitInfo);
	}
}
