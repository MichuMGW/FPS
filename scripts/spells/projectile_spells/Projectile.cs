using Godot;
using System;

public partial class Projectile : Node3D
{
	[Export] public float Speed;
	public Vector3 Velocity;

    public override void _PhysicsProcess(double delta)
    {
        GlobalTranslate(Velocity * (float)delta);

		var spaceState = GetWorld3D().DirectSpaceState;

		//Utworzenie zapytania promienia do wykrywania kolizji
		var query = new PhysicsRayQueryParameters3D()
		{
			From = GlobalTransform.Origin,	//Skąd zaczyna się promień
			To = GlobalTransform.Origin + Velocity.Normalized() * Speed * (float)delta,	//Dokąd promień zmierza (punkt w kierunku lotu)
			CollideWithBodies = true	//Pocisk sprawdza obiekty fizyczne (wrogowie, ściany itp.)
		};

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			Node3D hitObject = (Node3D)result["collider"];
			if (hitObject is CharacterBody3D characterBody3D)
			{
				var healthComponent = characterBody3D.GetNode("HealthComponent") as HealthComponent;
				healthComponent.TakeDamage(50);
			}
			QueueFree();
		}
    }


}
