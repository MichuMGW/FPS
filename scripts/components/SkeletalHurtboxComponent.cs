using Godot;
using System;
using System.Collections.Generic;

// Hurtbox dla modeli ze szkieletem – wiele HurtboxArea w BoneAttachmentach.
public partial class SkeletalHurtboxComponent : HurtboxComponent
{
    [Export] private string _armaturePath;

    private readonly List<HurtboxArea> _hurtboxes = new();

    public override void _Ready()
    {
        base._Ready();
        BuildFromSkeleton();
        Subscribe();
    }

    private void BuildFromSkeleton()
    {
        _hurtboxes.Clear();

        var armature = GetParent().GetNodeOrNull<Skeleton3D>(_armaturePath);
        if (armature == null)
        {
            GD.PushError($"{Name}: Nie znaleziono Skeleton3D pod ścieżką '{_armaturePath}'.");
            return;
        }

        foreach (Node3D node in armature.GetChildren())
        {
            if (node is BoneAttachment3D boneAttachment)
            {
                var hurtboxArea = boneAttachment.GetNodeOrNull<HurtboxArea>("HurtboxArea");
                if (hurtboxArea != null)
                {
                    SetHurtboxAreaOwner(hurtboxArea);
                    _hurtboxes.Add(hurtboxArea);
                }
            }
        }

        if (_hurtboxes.Count == 0)
        {
            GD.PushWarning($"{Name}: Nie znaleziono żadnych HurtboxArea w BoneAttachmentach.");
        }
    }

    private void Subscribe()
    {
        foreach (var hurtbox in _hurtboxes)
        {
            var local = hurtbox;
            local.BodyEntered += source => OnHurtboxBodyEntered(local, source);
            local.AreaEntered += source => OnHurtboxAreaEntered(local, source);
        }
    }

    private void OnHurtboxAreaEntered(HurtboxArea hurtbox, Area3D source)
    {
        ProcessHit(hurtbox, source);
    }

    private void OnHurtboxBodyEntered(HurtboxArea hurtbox, Node3D source)
    {
        ProcessHit(hurtbox, source);
    }

    public override void SetHurtboxesMonitoring(bool value)
    {
        foreach (var hurtbox in _hurtboxes)
        {
            hurtbox.SetDeferred("monitoring", value);
        }
    }

    public override void SetHutboxesMonitorable(bool value)
    {
        foreach (var hurtbox in _hurtboxes)
        {
            hurtbox.SetDeferred("monitorable", value);
        }
    }

    public override void SetHurtboxAreaOwner(HurtboxArea hurtboxArea)
    {
        hurtboxArea.OwnerHurtboxComponent = this;
    }

}
