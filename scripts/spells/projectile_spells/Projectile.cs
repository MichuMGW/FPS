using Godot;
using System;

public partial class Projectile : CharacterBody3D
{
    public (Element, Element) Elements {get; set;} //Żywioły zaklęcia
    public float Gravity {get; set;} // Grawitacja
    public float Damage {get; set;} // Obrażenia
    public float Range {get; set;} // Zasięg pocisku
    public Vector3 direction;
    public Vector3 startPosition;

    public override void _PhysicsProcess(double delta)
    {
        // Symulacja grawitacji
        //Velocity += new Vector3(0, Gravity * (float)delta, 0);

        // Przesunięcie pocisku i sprawdzenie kolizji
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta);

        if (startPosition.DistanceTo(GlobalTransform.Origin) > Range)
        {
            //TODO: Add animation
            QueueFree();
        }
        
        if (collision != null)
        {
            Node3D hitObject = (Node3D)collision.GetCollider();

            // if (hitObject is CharacterBody3D characterBody)
            // {
            //     var enemy = characterBody as Enemy;
            //     enemy.Health.TakeDamage(Damage);
            //     enemy.Health.ShowDamage(collision.GetPosition(), Damage, new Color(1, 1, 1));
            //     enemy.Status.ApplyStatusEffect(Elements.Item1, Elements.Item2);
            // }

            // Pocisk znika po kolizji
            QueueFree();
        }
    }
}

