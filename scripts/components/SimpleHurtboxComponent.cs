using System;
using Godot;

// Prosty byt – dokładnie jeden HurtboxArea.
public partial class SimpleHurtboxComponent : HurtboxComponent
{
    [Export] private NodePath _hurtboxPath;

    private HurtboxArea _hurtbox;

    public override void _Ready()
    {
        base._Ready();

        if (_hurtboxPath == null || _hurtboxPath.IsEmpty)
        {
            GD.PushError($"{Name}: HurtboxPath nie ustawiony w SimpleHurtboxComponent.");
            return;
        }

        _hurtbox = GetNodeOrNull<HurtboxArea>(_hurtboxPath);
        if (_hurtbox == null)
        {
            GD.PushError($"{Name}: Nie znaleziono HurtboxArea pod ścieżką '{_hurtboxPath}'.");
            return;
        }

        _hurtbox.BodyEntered += OnBodyEntered;
        _hurtbox.AreaEntered += OnAreaEntered;
    }


    private void OnBodyEntered(Node3D source)
    {
        ProcessHit(_hurtbox, source);
    }

    private void OnAreaEntered(Area3D source)
    {
        ProcessHit(_hurtbox, source);
    }

    public override void SetHurtboxesMonitoring(bool value)
    {
        _hurtbox.SetDeferred("monitoring", value);
    }

    public override void SetHutboxesMonitorable(bool value)
    {
        _hurtbox.SetDeferred("monitorable", value);
    }
}
