using Godot;

public interface IDamageSource
{
    float GetDamage();
    Element GetDamageType();
    bool CanHitAgain(Node3D target);  
    void RegisterHit(Node3D target);
}

//PRZYKŁADOWA IMOLEMENTACJA SPELLA
// public class AOEZone : Area3D, IDamageSource
// {
//     [Export] public float DamagePerTick = 5f;
//     [Export] public float TickRate = 0.2f;

//     private readonly Dictionary<Node3D, double> _lastHit = new();

//     public float GetDamage() => DamagePerTick;
//     public DamageType GetDamageType() => DamageType.Fire;

//     public bool CanHitAgain(Node3D target)
//     {
//         var now = Time.GetTicksMsec() / 1000.0;
//         return !_lastHit.TryGetValue(target, out var last) || now - last >= TickRate;
//     }

//     public void RegisterHit(Node3D target)
//     {
//         _lastHit[target] = Time.GetTicksMsec() / 1000.0;
//     }
// }
