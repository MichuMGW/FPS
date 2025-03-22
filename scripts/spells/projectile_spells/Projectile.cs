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
    [Export] public float Speed; // Początkowa prędkość
    [Export] public float Gravity = -2f; // Grawitacja
    [Export] public float Damage = 50f; // Obrażenia

    public override void _Ready()
    {
        Velocity = -Transform.Basis.Z * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Symulacja grawitacji
        //Velocity += new Vector3(0, Gravity * (float)delta, 0);

        // Przesunięcie pocisku i sprawdzenie kolizji
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta);
        
        if (collision != null)
        {
            Node3D hitObject = (Node3D)collision.GetCollider();

            if (hitObject is CharacterBody3D characterBody)
            {
                // Jeśli trafi w postać, zadaj obrażenia
                var healthComponent = characterBody.GetNodeOrNull<HealthComponent>("HealthComponent");
                healthComponent?.TakeDamage(Damage);
            }

            // Pocisk znika po kolizji
            QueueFree();
        }
    }
}

