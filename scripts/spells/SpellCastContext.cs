using Godot;

public struct SpellCastContext
{
    public Node3D Caster;
    public Node3D Muzzle;
    public Vector3 Direction;
    public SpellSlot Slot;
    public SpellInstance Instance;
    public SpellCastStats Stats;
    public float Charge01; // 0..1
}