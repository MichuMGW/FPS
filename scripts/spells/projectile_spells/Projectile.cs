using Godot;
using System;
using System.Collections.Generic;

public partial class Projectile : CharacterBody3D
{
    public Element Element {get; set;} //Żywioły zaklęcia
    // public Element Element {get; set; }
    public float Gravity {get; set;} // Grawitacja
    public float Damage {get; set;} // Obrażenia
    public float Range {get; set;} // Zasięg pocisku
    public float LifeTime {get; set;} = 5f;
    public Vector3 direction;
    public Vector3 startPosition;
    private float _timeAlive = 0f;

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

