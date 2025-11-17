using Godot;
using System;
using System.Collections.Generic;

public partial class Projectile : CharacterBody3D, IDamageSource
{
    public (Element, Element) Elements {get; set;} //Żywioły zaklęcia
    // public Element Element {get; set; }
    public float Gravity {get; set;} // Grawitacja
    public float Damage {get; set;} // Obrażenia
    public float Range {get; set;} // Zasięg pocisku
    public float LifeTime {get; set;} = 5f;
    public Vector3 direction;
    public Vector3 startPosition;
    private float _timeAlive = 0f;

    private readonly HashSet<Node3D> _alreadyHit = new();

    public float GetDamage() => Damage;
    // public Element GetDamageType() => Elements;
    public Element GetDamageType()
    {
        return Element.Fire;
    }
   

    public bool CanHitAgain(Node3D target) => !_alreadyHit.Contains(target);
    public void RegisterHit(Node3D target) => _alreadyHit.Add(target);

    public override void _PhysicsProcess(double delta)
    {
        // Symulacja grawitacji
        //Velocity += new Vector3(0, Gravity * (float)delta, 0);

        // Przesunięcie pocisku i sprawdzenie kolizji
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta);

        _timeAlive += (float)delta;

        if (_timeAlive >= LifeTime)
        {
            QueueFree();
        }

        if (startPosition.DistanceTo(GlobalTransform.Origin) > Range)
        {
            //TODO: Add animation
            QueueFree();
        }
        
        if (collision != null)
        {
            QueueFree();
        }
    }

}

