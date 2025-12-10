using Godot;

public struct SpellCastContext
{
    public Node3D Caster;
    public Node3D Muzzle;         // socket ręki
    public Vector3 Direction;
    public SpellInstance Instance;
    public SpellCastStats Stats;
}