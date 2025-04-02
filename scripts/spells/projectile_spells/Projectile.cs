// using Godot;
// using System;

// public partial class Projectile : Node3D
// {
// 	[Export] public float Speed;
// 	public Vector3 Velocity;
// 	private float gravity = -9.8f;

//     public override void _PhysicsProcess(double delta)
//     {
//         GlobalTranslate(Velocity * (float)delta);
		
// 		var spaceState = GetWorld3D().DirectSpaceState;

// 		//Utworzenie zapytania promienia do wykrywania kolizji
// 		var query = new PhysicsRayQueryParameters3D()
// 		{
// 			From = GlobalTransform.Origin,	//Skąd zaczyna się promień
// 			To = GlobalTransform.Origin + Velocity.Normalized() * Speed * (float)delta,	//Dokąd promień zmierza (punkt w kierunku lotu)
// 			CollideWithBodies = true	//Pocisk sprawdza obiekty fizyczne (wrogowie, ściany itp.)
// 		};

// 		var result = spaceState.IntersectRay(query);

// 		if (result.Count > 0)
// 		{
// 			Node3D hitObject = (Node3D)result["collider"];
// 			if (hitObject is CharacterBody3D characterBody3D)
// 			{
// 				var healthComponent = characterBody3D.GetNode("HealthComponent") as HealthComponent;
// 				healthComponent.TakeDamage(50);
// 			}
// 			QueueFree();
// 		}
//     }


// }

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

    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(double delta)
    {
        // Symulacja grawitacji
        //Velocity += new Vector3(0, Gravity * (float)delta, 0);

        // Przesunięcie pocisku i sprawdzenie kolizji
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta);

        if (startPosition.DistanceTo(GlobalTransform.Origin) > Range){
            //TODO: Add animation
            QueueFree();
        }
        
        if (collision != null)
        {
            Node3D hitObject = (Node3D)collision.GetCollider();

            if (hitObject is CharacterBody3D characterBody)
            {
                // Jeśli trafi w postać, zadaj obrażenia
                var healthComponent = characterBody.GetNodeOrNull<HealthComponent>("HealthComponent");
                healthComponent?.TakeDamage(Damage);
                healthComponent?.ShowDamage(collision.GetPosition() ,Damage);

                var statusComponent = characterBody.GetNodeOrNull<StatusComponent>("StatusComponent");
                statusComponent?.ApplyStatusEffect(Elements.Item1, Elements.Item2);
            }

            // Pocisk znika po kolizji
            QueueFree();
        }
    }
}

