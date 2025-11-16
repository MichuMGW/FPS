

using Godot;

public partial class HitInfo : Node
{
    public Node3D Source { get; set;}
    public HurtboxType HitboxType { get; set;}
    public float DamageMultiplier { get; set;}
    public Vector3 HitPosition { get; set;}

    public HitInfo(){}

    public HitInfo(Node3D source, HurtboxType hitboxType, float damageMultiplier, Vector3 hitPosition)
    {
        Source = source;
        HitboxType = hitboxType;
        DamageMultiplier = damageMultiplier;
        HitPosition = hitPosition;
    }
}